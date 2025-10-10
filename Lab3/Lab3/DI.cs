using Autofac;
using Autofac.Configuration;
using Lab3.Calculator;
using Lab3.Transaction;
using Lab3.UnitOfWork;
using Lab3.Worker;
using Microsoft.Extensions.Configuration;

public static class DI
{
    public static readonly string CatCalc = "catCalc";
    public static readonly string PlusCalc = "plusCalc";
    public static readonly string StateCalc = "stateCalc";
    public static readonly string StateWorker = "stateWorker";

    public static void ConfigureAll(ContainerBuilder builder)
    {
        ConfigureCalculators(builder);
        ConfigureWorkers(builder);
        ConfigureUnitOfWork(builder);
        ConfigureTransaction(builder);
    }

    public static void ConfigureCalculators(ContainerBuilder builder)
    {
        builder.RegisterType<CatCalc>().Named<ICalculator>(CatCalc);
        builder.RegisterType<PlusCalc>().Named<ICalculator>(PlusCalc);
        builder
            .RegisterType<StateCalc>()
            .Named<ICalculator>(StateCalc)
            .WithParameter("state", "0")
            .SingleInstance();
    }

    public static void ConfigureWorkers(ContainerBuilder builder)
    {
        // Worker 1
        builder
            .RegisterType<Worker>()
            .WithParameter(
                (pi, ctx) => pi.ParameterType == typeof(ICalculator),
                (pi, ctx) => ctx.ResolveNamed<ICalculator>(CatCalc)
            );
        builder
            .RegisterType<Worker>()
            .Named<Worker>(StateWorker)
            .WithParameter(
                (pi, ctx) => pi.ParameterType == typeof(ICalculator),
                (pi, ctx) => ctx.ResolveNamed<ICalculator>(StateCalc)
            );

        // Worker 2
        builder
            .RegisterType<Worker2>()
            .OnActivated(e =>
            {
                var calc = e.Context.ResolveNamed<ICalculator>(PlusCalc);
                e.Instance.SetCalculator(calc);
            });
        builder
            .RegisterType<Worker2>()
            .Named<Worker2>(StateWorker)
            .OnActivated(e =>
                e.Instance.SetCalculator(e.Context.ResolveNamed<ICalculator>(StateCalc))
            );

        // Worker 3
        builder
            .RegisterType<Worker3>()
            .OnActivated(e => e.Instance.Calculator = e.Context.ResolveNamed<ICalculator>(CatCalc));
        builder
            .RegisterType<Worker3>()
            .Named<Worker3>(StateWorker)
            .OnActivated(e =>
                e.Instance.Calculator = e.Context.ResolveNamed<ICalculator>(StateCalc)
            );
    }

    public static void ConfigureUnitOfWork(ContainerBuilder builder)
    {
        builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
    }

    public static void ConfigureTransaction(ContainerBuilder builder)
    {
        builder
            .RegisterType<TransactionContext>()
            .As<ITransactionContext>()
            .InstancePerMatchingLifetimeScope(TransactionProcessor.TransactionTag);
        builder.RegisterType<StepOneService>().AsSelf();
        builder.RegisterType<StepTwoService>().AsSelf();
        builder.RegisterType<TransactionProcessor>().AsSelf();
    }

    public static void ConfigureDeclarative(ContainerBuilder builder, IConfiguration config)
    {
        builder.RegisterModule(new ConfigurationModule(config.GetSection("autofac")));
        // Register more complex dependencies in code
        DI.ConfigureWorkers(builder);
    }
}
