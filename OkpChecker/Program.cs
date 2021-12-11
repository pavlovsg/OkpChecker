using System.Net.Sockets;

var buffer = new byte[] 
{ 
    0xDD, 0x80, 0xCA, 0xA1, // magic word DD80CAA1
    0x82, 0xA1, 0x00, 0x01, // S/P ver
    0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, 0x39, // fnId
    0x00, 0x00, // entry len 
    0x10, 0x00, // flags
    0x00, 0x00  // entry crc
};

var server = "prod01.okp-fn.ru";
var port = 26101;

var timeout = 30000;

Console.WriteLine($"Сформирован тестовый пакет к отправке размером {buffer.Length} байт:");
Console.WriteLine(BitConverter.ToString(buffer));

Console.WriteLine($"Попытка соединения с {server}:{port}");
try
{
    var client = new TcpClient(server, port);
    var stream = client.GetStream();
    stream.Write(buffer);
    Console.WriteLine($"Тестовый пакет отправлен к {server}:{port}");
    Console.WriteLine($"Ожидаем ответ до таймаута ({timeout} мс)...");

    Thread.Sleep(timeout);
    var dataAvailable = stream.DataAvailable;
    if (dataAvailable == false)
    {
        Console.WriteLine("Данные не поступили.");
        client.Close();
        Console.WriteLine("Соединение закрыто.");
        return;
    }
    var bytesAvailable = client.Available;
    var responseBuffer = new byte[bytesAvailable];
    var bytesCount = stream.Read(responseBuffer, 0, bytesAvailable);

    Console.WriteLine($"Пришёл ответ размером {bytesAvailable} байт:");
    Console.WriteLine(BitConverter.ToString(responseBuffer));

    var correctResponse = new byte[buffer.Length];
    Array.Copy(buffer, correctResponse, buffer.Length);
    correctResponse[correctResponse.Length - 4] = 0;
    Console.WriteLine(BitConverter.ToString(correctResponse));
    Console.WriteLine("^^^ Корректный ответ должен быть такой ^^^");
    if (responseBuffer.SequenceEqual(correctResponse))
    {
        Console.WriteLine("Ответ корректный.");
    }
    else
    {
        Console.WriteLine("Ответ некорректный.");
    }

    client.Close();
    Console.WriteLine("Соединение закрыто.");
}
catch (Exception ex)
{
    Console.WriteLine("Возникло исключение:");
    Console.WriteLine(ex.Message);
}