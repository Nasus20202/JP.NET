#include "DataModel.h"
#include <msclr\marshal_cppstd.h>

using namespace System::IO;
using namespace System::Linq;
using namespace System::Collections::Generic;
using namespace System::Reflection;
using namespace System::Text;
using namespace Microsoft::CodeAnalysis;

// Dodaj referencj? do DLL RoslynCompiler
#using "..\RoslynCompiler\bin\Debug\net8.0\RoslynCompiler.dll"

using namespace RoslynCompiler;

DataModel::DataModel(String^ codeText)
{
	_codeText = codeText;
}

DataModel::NavigateToAddCodeCommand::NavigateToAddCodeCommand(DataModel^ viewModel)
{
    _viewModel = viewModel;
}

bool DataModel::NavigateToAddCodeCommand::CanExecute(System::Object^ parameter) 
{
    return true;
}

void DataModel::NavigateToAddCodeCommand::Execute(System::Object^ parameter)
{
    _viewModel->ErrorsList->Clear();
    _viewModel->MethodsList->Clear();
    _viewModel->FieldsList->Clear();
    
    try {
        // Dodaj debug info
        _viewModel->ErrorsList->Add("Selected OutputKind: " + _viewModel->SelectedOutputKind.ToString());
        _viewModel->ErrorsList->Add("Code length: " + _viewModel->CodeText->Length.ToString() + " chars");
        
        // U?ycie klasy C# do kompilacji
        // SelectedOutputKind jest int, przekonwertuj na Microsoft.CodeAnalysis.OutputKind
        auto compilationResult = VBCompiler::CompileVBCode(
            _viewModel->CodeText, 
            (Microsoft::CodeAnalysis::OutputKind)_viewModel->SelectedOutputKind
        );
        
        if (!compilationResult->Success)
        {
            // Wy?wietlenie b??dów kompilacji
            for each (String^ error in compilationResult->Errors)
            {
                _viewModel->ErrorsList->Add(error);
            }
        }
        else
        {
            _viewModel->ErrorsList->Add("Compilation successful!");
            _viewModel->ErrorsList->Add("Assembly size: " + compilationResult->AssemblyBytes->Length.ToString() + " bytes");
            
            // Kompilacja powiod?a si? - za?aduj assembly
            auto assembly = Assembly::Load(compilationResult->AssemblyBytes);
            
            _viewModel->ErrorsList->Add("Assembly loaded: " + assembly->FullName);
            
            // Pobranie wszystkich typów (w??cznie z prywatnymi)
            auto allTypes = assembly->GetTypes();
            _viewModel->ErrorsList->Add("Total types found: " + allTypes->Length.ToString());
            
            // Debug - wy?wietl wszystkie typy
            for each (System::Type^ t in allTypes)
            {
                _viewModel->ErrorsList->Add("Type: " + t->FullName + " (IsPublic: " + t->IsPublic.ToString() + ")");
            }
            
            // Pobranie pierwszego typu publicznego
            auto types = assembly->GetExportedTypes();
            _viewModel->ErrorsList->Add("Exported types: " + types->Length.ToString());
            
            if (types->Length > 0 || allTypes->Length > 0)
            {
                // U?yj pierwszego dost?pnego typu
                _viewModel->Type = (types->Length > 0) ? types[0] : allTypes[0];
                
                _viewModel->ErrorsList->Add("Selected type: " + _viewModel->Type->FullName);
                
                // Utworzenie instancji klasy
                _viewModel->ObjectInstance = Activator::CreateInstance(_viewModel->Type);
                
                // Pobranie metod (bez metod odziedziczonych z Object)
                auto allMethods = _viewModel->Type->GetMethods(
                    BindingFlags::Public | BindingFlags::Instance | BindingFlags::DeclaredOnly
                );
                _viewModel->Methods = allMethods;
                
                // Pobranie pól
                auto allFields = _viewModel->Type->GetFields(
                    BindingFlags::Public | BindingFlags::Instance
                );
                _viewModel->Fields = allFields;
                
                _viewModel->ErrorsList->Add("Methods found: " + allMethods->Length.ToString());
                _viewModel->ErrorsList->Add("Fields found: " + allFields->Length.ToString());
            }
            else
            {
                _viewModel->ErrorsList->Add("No types found in compiled assembly");
            }
        }
    }
    catch (Exception^ ex)
    {
        _viewModel->ErrorsList->Add("Exception: " + ex->Message);
        if (ex->InnerException != nullptr) {
            _viewModel->ErrorsList->Add("Inner: " + ex->InnerException->Message);
        }
        _viewModel->ErrorsList->Add("Stack: " + ex->StackTrace);
    }
}

void DataModel::UpdateMethodsList()
{
    MethodsList->Clear();
    if (methods != nullptr)
    {
        for each (MethodInfo^ method in methods)
        {
            StringBuilder^ sb = gcnew StringBuilder();
            sb->Append(method->Name);
            sb->Append("(");
            
            auto params = method->GetParameters();
            for (int i = 0; i < params->Length; i++)
            {
                if (i > 0) sb->Append(", ");
                sb->Append(params[i]->ParameterType->Name);
                sb->Append(" ");
                sb->Append(params[i]->Name);
            }
            
            sb->Append(") : ");
            sb->Append(method->ReturnType->Name);
            
            MethodsList->Add(sb->ToString());
        }
    }
}

void DataModel::UpdateFieldsList()
{
    FieldsList->Clear();
    if (fields != nullptr)
    {
        for each (FieldInfo^ field in fields)
        {
            FieldsList->Add(field->Name + " : " + field->FieldType->Name);
        }
    }
}

void DataModel::UpdateFieldInfo()
{
    if (fields != nullptr && _selectedField != nullptr)
    {
        for each (FieldInfo^ field in fields)
        {
            String^ fieldStr = field->Name + " : " + field->FieldType->Name;
            if (fieldStr == _selectedField)
            {
                try {
                    FieldType = field->FieldType->Name;
                    Object^ value = field->GetValue(ObjectInstance);
                    FieldValue = (value != nullptr) ? value->ToString() : "null";
                }
                catch (Exception^ ex)
                {
                    FieldValue = "Error: " + ex->Message;
                }
                break;
            }
        }
    }
}

DataModel::InvokeMethodCommandClass::InvokeMethodCommandClass(DataModel^ viewModel)
{
    _viewModel = viewModel;
}

bool DataModel::InvokeMethodCommandClass::CanExecute(System::Object^ parameter)
{
    return true;
}

void DataModel::InvokeMethodCommandClass::Execute(System::Object^ parameter)
{
    if (_viewModel->methods == nullptr || _viewModel->SelectedMethod == nullptr)
    {
        _viewModel->MethodResult = "No method selected";
        return;
    }
    
    try {
        MethodInfo^ selectedMethodInfo = nullptr;
        for each (MethodInfo^ method in _viewModel->methods)
        {
            StringBuilder^ sb = gcnew StringBuilder();
            sb->Append(method->Name);
            sb->Append("(");
            
            auto params = method->GetParameters();
            for (int i = 0; i < params->Length; i++)
            {
                if (i > 0) sb->Append(", ");
                sb->Append(params[i]->ParameterType->Name);
                sb->Append(" ");
                sb->Append(params[i]->Name);
            }
            
            sb->Append(") : ");
            sb->Append(method->ReturnType->Name);
            
            if (sb->ToString() == _viewModel->SelectedMethod)
            {
                selectedMethodInfo = method;
                break;
            }
        }
        
        if (selectedMethodInfo != nullptr)
        {
            auto params = selectedMethodInfo->GetParameters();
            array<Object^>^ args = nullptr;
            
            if (params->Length > 0)
            {
                args = gcnew array<Object^>(params->Length);
                
                if (!String::IsNullOrEmpty(_viewModel->MethodParameter))
                {
                    args[0] = _viewModel->MethodParameter;
                }
            }
            
            Object^ result = selectedMethodInfo->Invoke(_viewModel->ObjectInstance, args);
            _viewModel->MethodResult = (result != nullptr) ? result->ToString() : "null";
        }
        else
        {
            _viewModel->MethodResult = "Method not found";
        }
    }
    catch (Exception^ ex)
    {
        _viewModel->MethodResult = "Error: " + ex->Message;
    }
}

DataModel::SetFieldCommandClass::SetFieldCommandClass(DataModel^ viewModel)
{
    _viewModel = viewModel;
}

bool DataModel::SetFieldCommandClass::CanExecute(System::Object^ parameter)
{
    return true;
}

void DataModel::SetFieldCommandClass::Execute(System::Object^ parameter)
{
    if (_viewModel->fields == nullptr || _viewModel->SelectedField == nullptr)
    {
        return;
    }
    
    try {
        for each (FieldInfo^ field in _viewModel->fields)
        {
            String^ fieldStr = field->Name + " : " + field->FieldType->Name;
            if (fieldStr == _viewModel->SelectedField)
            {
                Object^ value = nullptr;
                
                if (field->FieldType == String::typeid)
                {
                    value = _viewModel->FieldValue;
                }
                else if (field->FieldType == int::typeid || field->FieldType == Int32::typeid)
                {
                    value = Int32::Parse(_viewModel->FieldValue);
                }
                else if (field->FieldType == bool::typeid || field->FieldType == Boolean::typeid)
                {
                    value = Boolean::Parse(_viewModel->FieldValue);
                }
                else if (field->FieldType == double::typeid || field->FieldType == Double::typeid)
                {
                    value = Double::Parse(_viewModel->FieldValue);
                }
                
                field->SetValue(_viewModel->ObjectInstance, value);
                
                _viewModel->UpdateFieldInfo();
                break;
            }
        }
    }
    catch (Exception^ ex)
    {
        _viewModel->FieldValue = "Error: " + ex->Message;
    }
}


