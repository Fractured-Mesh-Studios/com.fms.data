using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataEngine
{
    using System;
    using System.IO;
    using System.Threading; // Para el Thread.Sleep en el ejemplo

    public class FileMonitor
    {
        private FileSystemWatcher watcher;
        private string filePathToMonitor;
        private DateTime lastWriteTime; // Para rastrear la última escritura y detectar "fin"
        private Timer timer; // Para un temporizador que detecte inactividad

        public event EventHandler FileFinishedWriting;

        public FileMonitor(string directoryPath, string fileName)
        {
            filePathToMonitor = Path.Combine(directoryPath, fileName);

            watcher = new FileSystemWatcher(directoryPath);

            // Monitorea solo el archivo específico
            watcher.Filter = fileName;

            // Monitorea cambios en el tamaño y la hora de última escritura
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;

            // Suscribirse al evento Changed
            watcher.Changed += OnFileChanged;

            // Opcional: si el archivo se crea durante la ejecución
            watcher.Created += OnFileCreated;

            // Habilitar el observador
            watcher.EnableRaisingEvents = true;

            Debug.Log($"Monitoreando cambios en: {filePathToMonitor}");

            // Inicializar el temporizador, pero no lo iniciaré hasta que detecte un cambio
            // Se disparará si no hay cambios en un cierto período (ej. 1 segundo)
            timer = new Timer(CheckFileInactivity, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == filePathToMonitor)
            {
                Debug.Log($"Archivo creado: {e.FullPath}");
                // Resetear el temporizador y la última hora de escritura
                lastWriteTime = File.GetLastWriteTime(filePathToMonitor);
                timer.Change(1000, Timeout.Infinite); // Reinicia el temporizador a 1 segundo
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == filePathToMonitor)
            {
                Debug.Log($"Archivo modificado: {e.FullPath}");
                // Actualizar la última hora de escritura
                lastWriteTime = File.GetLastWriteTime(filePathToMonitor);
                // Reiniciar el temporizador para esperar más cambios
                timer.Change(1000, Timeout.Infinite); // Reinicia el temporizador a 1 segundo
            }
        }

        private void CheckFileInactivity(object state)
        {
            // Este método se ejecuta cuando el temporizador se dispara (es decir, no ha habido cambios recientes)
            try
            {
                // Volver a verificar la última hora de escritura para asegurarnos de que no hubo un cambio justo antes del temporizador
                DateTime currentLastWriteTime = File.GetLastWriteTime(filePathToMonitor);

                if (currentLastWriteTime == lastWriteTime)
                {
                    // Si la última hora de escritura no ha cambiado desde la última vez que se detectó un cambio
                    // O si el archivo se creó y no ha habido más escrituras
                    Debug.Log($"¡Escritura del archivo {filePathToMonitor} parece haber terminado!");
                    FileFinishedWriting?.Invoke(this, EventArgs.Empty); // Disparar el evento
                    timer.Change(Timeout.Infinite, Timeout.Infinite); // Detener el temporizador
                }
                else
                {
                    // Hubo un cambio justo antes de que se disparara el temporizador, reiniciar
                    lastWriteTime = currentLastWriteTime;
                    timer.Change(1000, Timeout.Infinite);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.Log($"El archivo {filePathToMonitor} ya no existe.");
                timer.Change(Timeout.Infinite, Timeout.Infinite); // Detener el temporizador
            }
            catch (Exception ex)
            {
                Debug.Log($"Error al verificar inactividad del archivo: {ex.Message}");
                timer.Change(Timeout.Infinite, Timeout.Infinite); // Detener el temporizador
            }
        }

        public void StopMonitoring()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            timer.Dispose();

            Debug.Log("Monitoreo detenido.");
        }
    }
}

/* Ejemplo de uso (Example)

public static void Main(string[] args)
{
    string directory = "C:\\Temp"; // Cambia esto a un directorio real
    string file = "test_file.txt";

    // Asegúrate de que el directorio exista
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    FileMonitor monitor = new FileMonitor(directory, file);
    monitor.FileFinishedWriting += (s, e) =>
    {
        Console.WriteLine("¡Evento: El archivo ha terminado de escribirse!");
        // Aquí puedes realizar acciones una vez que el archivo ha terminado de escribirse
    };

    Console.WriteLine("Presiona cualquier tecla para crear/modificar el archivo...");
    Console.ReadKey();

    // Simular escritura de archivo
    Console.WriteLine("\nEscribiendo en el archivo...");
    using (StreamWriter sw = new StreamWriter(Path.Combine(directory, file), true))
    {
        sw.WriteLine("Primera línea.");
        Thread.Sleep(500); // Pequeña pausa
        sw.WriteLine("Segunda línea.");
        Thread.Sleep(500); // Pequeña pausa
        sw.WriteLine("Tercera línea.");
    }
    Console.WriteLine("Escritura inicial completada.");

    // Simular otra escritura después de un tiempo
    Thread.Sleep(3000); // Esperar un poco
    Console.WriteLine("\nEscribiendo más en el archivo...");
    using (StreamWriter sw = new StreamWriter(Path.Combine(directory, file), true))
    {
        sw.WriteLine("Cuarta línea.");
    }
    Console.WriteLine("Segunda escritura completada.");


    Console.WriteLine("\nPresiona cualquier tecla para salir...");
    Console.ReadKey();
    monitor.StopMonitoring();
}
 */
