using Copy_logEAS.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Copy_logEAS
{

    //Наследование  Form1 : ShadowedForm от кастомного контрола,
    //представляющего контрол формы
    //с возможностью настройки шапки формы и кнопок меню окна

    public partial class Form1 : ShadowedForm
    {
        // Переменные 

        int counter = 0;                 //Счетчик файлов 
        string pathRoot = @"D:\CopyLOG\";   //Путь создания исходной, временной папки
                                            //для копирования туда выбранных файлов
                                            //после отработки метода CopyLogEas

        public Form1()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now;
            Animator.Start();
        }

        //Метод для копирования логов 

        private void CopyLogEas(string targetDirectory, string Name)
        {
            try
            {
                List<DateTime> dates = new List<DateTime>();
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (var d in fileEntries)
                {
                    //Исходя из значения даты, производится выборка для копирования логов

                    if (dateTimePicker1.Value.ToString("dd.MM.yyyy") == File.GetLastWriteTime(d).ToString("dd.MM.yyyy"))
                    {
                        counter++;
                        dates.Add(File.GetLastWriteTime(d));
                        Directory.CreateDirectory(pathRoot + Name);
                        FileInfo fileInfo = new FileInfo(d);
                        string date = dateTimePicker1.Value.ToString("dd.MM.yyyy");
                        string path = pathRoot + Name + @"\" + Name + " " + d;
                        File.Copy(d, pathRoot + Name + "\\" + Name + "_" + date + fileInfo.Extension.ToString(), true);
                    }

                    //Папка Fiscal мало весит и копируется вся, со всеми ее файлами,
                    //но это случается только если найден хоть один файл с датой,
                    //меньшей, либо равной дате, указанной при запуске программы

                    if (Name == "Fiscal" & 
                        dateTimePicker1.Value.Day <= File.GetLastWriteTime(d).Day &
                        dateTimePicker1.Value.Month <= File.GetLastWriteTime(d).Month)
                    {
                        counter++;
                        dates.Add(File.GetLastWriteTime(d));
                        FileSystem.CopyDirectory(targetDirectory, pathRoot + "Fiscal", true);
                    }
                }

                //Вывод массива имен и дат, файлов которые добавлены в лист дат в прошлом цикле
                //Это те даты и имена файлов которые прошли условие в прошлом цикле и скопировались

                for (int i = 0; i < dates.Count; i++)
                {
                    listBox1.Items.Add((dates[i]) + " " + Name + " - добавлен");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}");
            }
        }

        // Кнопка Перейти в папку с ахривом
        private void yt_Button3_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"D:\CopyLogArchive\");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}");
            }
        }

        // Кнопка Сегодня 
        private void yt_Button2_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
        }

        // Кнопка Собрать логи ЕАС 
        async private void yt_Button1_Click(object sender, EventArgs e)
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;
                Directory.CreateDirectory(pathRoot);
                listBox1.Items.Clear();

                //Массив путей, откуда начинается копирование

                string[] massPath = new string[] { 
           @"C:\ProgramData\Pos\logs",
            @"C:\ProgramData\Russian.Post.EAS4\Cart",
             @"C:\ProgramData\Russian.Post.EAS4\Cash",
              @"C:\ProgramData\Russian.Post.EAS4\Fiscal",
               @"C:\ProgramData\Russian.Post.EAS4\Pinpad",
                @"C:\ProgramData\Russian.Post.MARS\Discovery",
                 @"C:\ProgramData\Russian.Post.MARS\Discovery\archive",
                  @"C:\ProgramData\Russian.Post.MARS\Downloader",
                   @"C:\ProgramData\Russian.Post.MARS\Launcher",
                    @"C:\ProgramData\Russian.Post.MARS\Launcher\archive",
                     @"C:\ProgramData\Russian.Post.MARS\NSS\logs",
                      @"C:\ProgramData\Russian.Post.MARS\PerformanceCounter",
                       @"C:\ProgramData\Russian.Post.MARS\PerformanceCounter\archive",
                        @"C:\ProgramData\Russian.Post.MARS\Replicator",
                         @"C:\ProgramData\Russian.Post.MARS\Updater",
                          @"C:\ProgramData\Russian.Post.MARS\Updater\archive",
                           @"C:\ProgramData\Russian.Post.MARS\Watchdog",
                            @"C:\ProgramData\Russian.Post.MARS\Watchdog\archive"
            };

                //Массив имен, которыми будут названы скопированные файлы

                string[] massName = new string[] { "EAS_log", "Cart", "Cash", "Fiscal", "Pinpad",
                "Discovery", "DiscoveryArchive", "Downloader", "Launcher", "LauncherArchive",
                "NSS","PerformanceCounter","PerformanceCounterArchive","Replicator","Updater",
            "UpdaterArchive","Watchdog","WatchdogArchive"};

                //Цикл, крутящий метод CopyLogEas, заполняющий его параметры данными, из массиов выше

                for (int i = 0; i < massPath.Length; i++)
                {
                    await Task.Run(() => CopyLogEas(massPath[i], massName[i]));
                    progressBar1.Value += 5;
                }

                string date = DateTime.Now.ToString("dd.MM.yyyy.HH.mm.ss");
                string zipFile = @"D:\CopyLOG " + date + ".zip"; // сжатый файл
                string zipFile1 = @"D:\CopyLogArchive\CopyLOG " + date + ".zip"; // сжатый файл
                FileInfo fileInf1 = new FileInfo(zipFile);
                FileInfo fileInf2 = new FileInfo(zipFile1);

                //Удаляется файл заархивированных логов

                if (fileInf1.Exists)
                {
                    fileInf1.Delete();
                }
                if (fileInf2.Exists)
                {
                    fileInf2.Delete();
                }

                //Создается директория, в которую поместится будующий архив

                Directory.CreateDirectory(@"D:\CopyLogArchive\");

                string sourceFolder = pathRoot; // исходная папка 
                string time = dateTimePicker1.Value.ToString("dd.MM.yyyy");

                //Происходит архивирование,
                //созданной временной папки для копирования логов,
                //после отработки метода CopyLogEas все файлы поступают в эту папку
                //По пути pathRoot

                await Task.Run(() => ZipFile.CreateFromDirectory(sourceFolder, zipFile));


                string path = zipFile;
                string newPath = @"D:\CopyLogArchive\CopyLOG от " + time + " создано " + date + ".zip";
                FileInfo fileInf3 = new FileInfo(path);

                //Пока архив до конца не создался, цикл будет крутится и ждать 

                while (fileInf3.Exists == false)
                {
                    await Task.Delay(500);
                }

                //Как только архив создался, он будет перемещен по пути newPath

                fileInf3.MoveTo(newPath);

                //Архив создан, папка логов,
                //которая была создана методом CopyLogEas (отбором логов) и заархивирована, будет удалена
                //Удаление исходной, временно папки по пути pathRoot

                string dirName = @"D:\CopyLOG";
                DirectoryInfo dirInfo = new DirectoryInfo(dirName);
                if (dirInfo.Exists)
                {
                    dirInfo.Delete(true);
                }
                progressBar1.Value = 100;
                MessageBox.Show("Архивирование выполнено, файлов в архиве " + counter);
                progressBar1.Value = 0;

                //Счетчик файлов обнулен

                counter = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex}");
            }
        }
    }
}
