using System.Composition;
using Prism.Events;

namespace Lab4.DashboardApp;

[Export(typeof(IEventAggregator))]
[Shared]
public class CustomEventAggregator : EventAggregator { }
