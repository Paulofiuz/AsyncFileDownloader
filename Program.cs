using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    public static async Task Main(string[] args)
    {
        await ExecutaOperacaoAsync();
    }

    static async Task ExecutaOperacaoAsync()
    {
        var tempo = 150;
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(tempo));

        Console.WriteLine("Iniciando o download...");
        Console.WriteLine($"Cancelando a operação após {tempo} segundos.");

        try
        {
            using var httpClient = new HttpClient();
            var destino = "C:\\dev\\AppBaixar\\arquivo.txt";

            var response = await httpClient.GetAsync(
                "https://www.macoratti.net/dados/Poesia.txt",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationTokenSource.Token
            );

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var readBytes = 0L;

            await using var fileStream = new FileStream(destino, FileMode.Create, FileAccess.Write);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationTokenSource.Token);

            var buffer = new byte[81920];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationTokenSource.Token);
                readBytes += bytesRead;
                Console.WriteLine($"Progresso: {readBytes}/{totalBytes} bytes");
            }
        }
        catch (OperationCanceledException ex)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Download cancelado pelo usuário: " + ex.Message);
            }
            else
            {
                Console.WriteLine("O limite de tempo para o download foi atingido!");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Ocorreu um erro de rede: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocorreu um erro desconhecido: " + ex.Message);
        }
        finally
        {
            Console.WriteLine("Download finalizado!");
        }
    }
}
