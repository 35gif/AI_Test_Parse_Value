using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AI_Test_Parse_Value
{
    internal class Program
    {
        public class Neuron
        {
            public decimal Weight { get; private set; } = 0.1m;
            public decimal LastError { get; private set; }
            public decimal Smoothing { get; set; } = 0.000001m;

            public decimal ProcessInputData(decimal input)
            {
                return input * Weight;
            }

            public decimal RestoreInputData(decimal output)
            {
                return output / Weight;
            }

            public void Train(decimal input, decimal expectedResult)
            {
                var actualResult = input * Weight;
                LastError = expectedResult - actualResult;
                var correction = (LastError / actualResult) * Smoothing;
                Weight += correction;
            }

            public void SaveWeight(string filePath)
            {
                File.WriteAllText(filePath, Weight.ToString());
            }

            public void LoadWeight(string filePath)
            {
                if (File.Exists(filePath))
                {
                    string weightString = File.ReadAllText(filePath);
                    decimal.TryParse(weightString, out decimal weight);
                    Weight = weight;
                }
            }
        }

        static async Task Main(string[] args)
        {
            decimal usd = 100;
            decimal byn = 0;

            Neuron neuron = new Neuron();

            string weightFilePath = "weight.txt";

            bool continueProgram = true;

            while (continueProgram)
            {
                Console.WriteLine("Выберите опцию:");
                Console.WriteLine("1. Переобучить модель");
                Console.WriteLine("2. Продолжить без переобучения");
                Console.WriteLine("3. Вывести файл весов");
                Console.WriteLine("4. Выйти");

                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "1":
                        string apiKey = "7UNXkGXleFZMIONUAiieRqe6FF2WHUqs";
                        CurrencyApi currencyApi = new CurrencyApi(apiKey);

                        decimal response = await currencyApi.GetExchangeRate("USD", "BYN", 1);

                        // Обработка ответа и извлечение необходимых данных из API
                        if (response != 0)
                        {
                            byn = usd * response;
                        }

                        // Обработка ответа и извлечение необходимых данных из API
                        if (!string.IsNullOrEmpty(response.ToString()))
                        {
                            if (decimal.TryParse(response.ToString(), out decimal rate))
                            {
                                byn = usd * rate;
                            }
                            else
                            {
                                Console.WriteLine("Ошибка при преобразовании значения обменного курса.");
                            }
                        }

                        neuron = new Neuron();

                        int i = 0;
                        do
                        {
                            i++;
                            neuron.Train(usd, byn);
                            if (i % 10000000 == 0)
                            {
                                Console.WriteLine($"Итерация: {i}\t{neuron.LastError}");
                            }
                        } while (neuron.LastError > neuron.Smoothing || neuron.LastError < -neuron.Smoothing);

                        neuron.SaveWeight(weightFilePath);

                        Console.WriteLine("Обучалка пройдена");
                        break;

                    case "2":
                        neuron.LoadWeight(weightFilePath);
                        Console.WriteLine("Продолжено без переобучения");
                        break;

                    case "3":
                        try
                        {
                            using (StreamReader sr = new StreamReader("weight.txt"))
                            {
                                string fileContent = sr.ReadToEnd();
                                // Дальнейшая обработка содержимого файла
                                Console.WriteLine("Файл весов успешно открыт.");
                                Console.WriteLine(fileContent);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при открытии файла: {ex.Message}");
                        }
                        break;

                    case "4":
                        continueProgram = false;
                        Console.WriteLine("Программа завершена");
                        break;

                    default:
                        Console.WriteLine("Неправильный выбор опции. Пожалуйста, выберите снова.");
                        break;
                }

                if (continueProgram)
                {
                    Console.WriteLine("Выберите данные для вывода:");
                    Console.WriteLine("1. USD в BY");
                    Console.WriteLine("2. BY в USD");
                    Console.WriteLine("3. Назад");

                    string outputChoice = Console.ReadLine();

                    decimal inputAmount;
                    decimal outputAmount;

                    switch (outputChoice)
                    {
                        case "1":
                            Console.Write("Введите сумму в USD: ");
                            inputAmount = decimal.Parse(Console.ReadLine());
                            outputAmount = neuron.ProcessInputData(inputAmount);
                            Console.WriteLine($"{inputAmount} USD в {outputAmount} BY");
                            break;

                        case "2":
                            Console.Write("Введите сумму в BY: ");
                            inputAmount = decimal.Parse(Console.ReadLine());
                            outputAmount = neuron.RestoreInputData(inputAmount);
                            Console.WriteLine($"{inputAmount} BY в {outputAmount} USD");
                            break;

                        case "3":

                            break;

                        default:
                            Console.WriteLine("Неправильный выбор данных для вывода.");
                            break;
                    }
                }
            }
        }
    }
    public class CurrencyApi
    {
        private const string ApiUrl = "https://api.apilayer.com/exchangerates_data/convert";
        private readonly string apiKey;

        public CurrencyApi(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency, decimal amount)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", apiKey);

                string requestUrl = $"{ApiUrl}?from={fromCurrency}&to={toCurrency}&amount={amount}";

                HttpResponseMessage response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Обработка ответа JSON и извлечение значения "result"
                    JObject responseObject = JObject.Parse(responseContent);
                    decimal result = (decimal)responseObject["result"];

                    return result;
                }
                else
                {
                    Console.WriteLine("Произошла ошибка при получении данных от API.");
                    return 0; // Возвращайте значение по умолчанию или выберите подходящее значение для обработки ошибки.
                }
            }
        }
    }
}

//7UNXkGXleFZMIONUAiieRqe6FF2WHUqs