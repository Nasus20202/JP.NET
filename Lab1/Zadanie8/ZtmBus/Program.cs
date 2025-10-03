using System.CommandLine;
using ZtmBus.Models;
using ZtmBus.Services;

var rootCommand = new RootCommand("Narzędzie CLI do sprawdzania rozkładów ZTM w Gdańsku");

var stopNameArgument = new Argument<string>("stop-name", "Nazwa przystanku do wyszukania");
var listStopsOption = new Option<bool>(
    new[] { "--list", "-l" },
    "Wyświetl listę wszystkich przystanków pasujących do wyszukiwanego terminu"
);
var exactOption = new Option<bool>(new[] { "--exact", "-e" }, "Wyszukaj dokładną nazwę przystanku");

rootCommand.Add(stopNameArgument);
rootCommand.Add(listStopsOption);
rootCommand.Add(exactOption);

rootCommand.SetHandler(
    async (string stopName, bool listStops, bool exact) =>
    {
        var httpClient = new HttpClient();
        var ztmService = new ZtmApiService(httpClient);

        try
        {
            Console.WriteLine("Pobieranie listy przystanków...");
            var allStops = await ztmService.GetStopsAsync();

            if (!allStops.Any())
            {
                Console.WriteLine("Nie udało się pobrać listy przystanków.");
                Environment.Exit(1);
            }

            Console.WriteLine($"Wyszukiwanie przystanków zawierających: '{stopName}'");
            var matchingStops = ztmService.FindStopsByName(allStops, stopName);

            if (!matchingStops.Any())
            {
                Console.WriteLine($"Nie znaleziono przystanków pasujących do: '{stopName}'");
                Environment.Exit(1);
            }

            if (listStops)
            {
                Console.WriteLine($"\nZnaleziono {matchingStops.Count} przystanków:");
                Console.WriteLine(new string('-', 80));
                foreach (var stop in matchingStops.Take(20)) // Pokaż maksymalnie 20 wyników
                {
                    Console.WriteLine(
                        $"ID: {stop.StopId:D4} | {stop.StopDesc} ({stop.ZoneName}) [{stop.Type}]"
                    );
                }

                if (matchingStops.Count > 20)
                {
                    Console.WriteLine(
                        $"... i {matchingStops.Count - 20} więcej. Użyj bardziej specyficznej nazwy."
                    );
                }
                return;
            }

            if (matchingStops.Count > 1)
            {
                Console.WriteLine($"\nZnaleziono {matchingStops.Count} przystanków:");
                Console.WriteLine(new string('-', 80));
                foreach (var stop in matchingStops.Take(10)) // Pokaż maksymalnie 10 wyników
                {
                    Console.WriteLine(
                        $"ID: {stop.StopId:D4} | {stop.StopDesc} ({stop.ZoneName}) [{stop.Type}]"
                    );
                }

                if (matchingStops.Count > 10)
                {
                    Console.WriteLine($"... i {matchingStops.Count - 10} więcej.");
                }

                Console.WriteLine(
                    "\nZbyt wiele wyników. Użyj --list aby zobaczyć wszystkie lub bardziej specyficznej nazwy."
                );
                Console.WriteLine("Wybieram pierwszy przystanek z listy...\n");
            }

            var selectedStop = matchingStops.First();
            Console.WriteLine(
                $"\nPobieranie rozkładu dla przystanku: {selectedStop.StopDesc} (ID: {selectedStop.StopId})"
            );
            Console.WriteLine(new string('-', 80));

            var departures = await ztmService.GetDeparturesAsync(selectedStop.StopId);

            if (!departures.Departures.Any())
            {
                Console.WriteLine("Brak dostępnych odjazdów dla tego przystanku.");
                return;
            }

            Console.WriteLine(
                $"Ostatnia aktualizacja: {departures.LastUpdate:yyyy-MM-dd HH:mm:ss}"
            );
            Console.WriteLine();
            Console.WriteLine(
                "Linia | Kierunek                    | Czas planowany | Czas estymowany | Opóźnienie | Status"
            );
            Console.WriteLine(new string('-', 95));

            foreach (var departure in departures.Departures.Take(15)) // Pokaż najbliższych 15 odjazdów
            {
                var theoreticalTime = departure.TheoreticalTime.ToString("HH:mm");
                var estimatedTime = departure.EstimatedTime.ToString("HH:mm");
                var delay = departure.DelayInSeconds.HasValue
                    ? $"{departure.DelayInSeconds / 60:+#;-#;0} min"
                    : "---";
                var status = departure.Status == "REALTIME" ? "Na żywo" : "Rozkład";

                // Skróć kierunek jeśli jest za długi
                var headsign =
                    departure.Headsign.Length > 25
                        ? departure.Headsign.Substring(0, 22) + "..."
                        : departure.Headsign;

                Console.WriteLine(
                    $"{departure.RouteShortName, -5} | {headsign, -25} | {theoreticalTime, -13} | {estimatedTime, -14} | {delay, -9} | {status}"
                );
            }

            if (departures.Departures.Count > 15)
            {
                Console.WriteLine($"\n... i {departures.Departures.Count - 15} więcej odjazdów.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            httpClient.Dispose();
        }
    },
    stopNameArgument,
    listStopsOption,
    exactOption
);

await rootCommand.InvokeAsync(args);
