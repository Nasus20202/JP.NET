#!/bin/bash

dotnet cake --target=CI
dotnet tool install --global --add-source ./publish ZtmBus.Cli
ztmbus "Politechnika"