using LibreHardwareMonitor.Hardware;
using System;
using System.Linq;
using System.Timers;

namespace TemperatureMonitor
{
    class Program
    {
        private static System.Timers.Timer _timer;

        static void Main(string[] args)
        {
            Console.WriteLine("Инициализация мониторинга оборудования...");
            
            var computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true
            };

            computer.Open();
            System.Threading.Thread.Sleep(1000); // Задержка для инициализации

            // Первое immediate чтение
            Console.WriteLine("\n=== Первое чтение ===");
            GetReadings(computer);
            
            // Запускаем таймер для периодического обновления
            _timer = new System.Timers.Timer(2000);
            _timer.Elapsed += (sender, e) => GetReadings(computer);
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Console.WriteLine("\nМониторинг запущен. Нажмите любую клавишу для выхода...");
            Console.ReadKey();

            computer.Close();
            _timer.Stop();
            _timer.Dispose();
        }

        private static void GetReadings(Computer computer)
        {
            Console.Clear();
            Console.WriteLine($"=== Данные с датчиков ({DateTime.Now:HH:mm:ss}) ===\n");
            
            bool anyDataFound = false;

            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                
                Console.WriteLine($"Устройство: {hardware.Name} ({hardware.HardwareType})");
                
                var temperatureSensors = hardware.Sensors
                    .Where(s => s.SensorType == SensorType.Temperature)
                    .ToList();

                if (temperatureSensors.Any())
                {
                    anyDataFound = true;
                    foreach (var sensor in temperatureSensors)
                    {
                        string value = sensor.Value.HasValue ? 
                            $"{sensor.Value.Value:0.0} °C" : "N/A";
                            
                        Console.WriteLine($"  {sensor.Name}: {value}");
                    }
                }
                else
                {
                    Console.WriteLine("  Температурные датчики не найдены");
                }
                Console.WriteLine();
            }

            if (!anyDataFound)
            {
                Console.WriteLine("ВНИМАНИЕ: Не найдено ни одного датчика температуры!");
                Console.WriteLine("Возможные причины:");
                Console.WriteLine("1. Запустите программу от имени Администратора");
                Console.WriteLine("2. Оборудование не поддерживается библиотекой");
                Console.WriteLine("3. Проблема с драйверами");
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
        }
    }
}