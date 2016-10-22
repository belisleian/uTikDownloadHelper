using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace uTikDownloadHelper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            OpenFileDialog ofdTik = new OpenFileDialog();
            ofdTik.Filter = ".tik File|*.tik";
            string ticketInPath;
            if (args.Length != 1) {
                if (ofdTik.ShowDialog() == DialogResult.OK)
                {
                    ticketInPath = ofdTik.FileName;
                }
                else
                    return;
            } else
            {
                ticketInPath = args[0];
            }

            Byte[] ticket = File.ReadAllBytes(ticketInPath);
            patchTicket(ref ticket);
            string hexID = "";

            for (int i = 0x1DC; i < 0x1DC + 8; i++)
            {
                hexID += string.Format("{0:X2}", ticket[i]);
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the folder to download the files to.";

            ChooseFolder:
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                int directoryItems = 0;
                directoryItems += Directory.GetFiles(fbd.SelectedPath).Length;
                directoryItems += Directory.GetDirectories(fbd.SelectedPath).Length;

                string nusGrabberPath = Path.Combine(fbd.SelectedPath, "NUSgrabber.exe");
                string wgetPath = Path.Combine(fbd.SelectedPath, "wget.exe");
                string vcRuntime140Path = Path.Combine(fbd.SelectedPath, "vcruntime140.dll");

                if (directoryItems == 0)
                {
                    File.WriteAllBytes(nusGrabberPath, uTikDownloadHelper.Properties.Resources.NUSgrabber);
                    File.WriteAllBytes(wgetPath, uTikDownloadHelper.Properties.Resources.wget);
                    File.WriteAllBytes(vcRuntime140Path, uTikDownloadHelper.Properties.Resources.vcruntime140);

                    var procStIfo = new ProcessStartInfo(nusGrabberPath, hexID);
                    procStIfo.RedirectStandardOutput = false;
                    procStIfo.UseShellExecute = false;
                    procStIfo.CreateNoWindow = false;
                    procStIfo.WorkingDirectory = fbd.SelectedPath;

                    var proc = new Process();
                    proc.StartInfo = procStIfo;
                    proc.Start();

                    proc.WaitForExit();

                    File.Delete(nusGrabberPath);
                    File.Delete(wgetPath);
                    File.Delete(vcRuntime140Path);

                    string downloadPath = fbd.SelectedPath + "\\" + hexID;
                    if (Directory.Exists(downloadPath))
                    {
                        foreach (string file in Directory.GetFiles(downloadPath))
                        {
                            string name = Path.GetFileName(file);

                            File.Move(file, Path.Combine(fbd.SelectedPath, name));
                        }
                        Directory.Delete(downloadPath);
                    }

                    File.WriteAllBytes(Path.Combine(fbd.SelectedPath, "title.tik"), ticket);

                } else
                {
                    if (MessageBox.Show("The selected folder isn't empty, do you want to choose another folder?", "Folder Not Empty",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        goto ChooseFolder;
                    }
                }
            }
        }

        static void patchTicket(ref Byte[] bytes)
        {
            if (bytes[0x01] == 0x3)
            {
                bytes[0x1] = 1;
                bytes[0xF] = (byte)((int)bytes[0xF] ^ 2);
            }
        }

        static void writeBytes(Byte[] bytes, Stream output)
        {
            output.Write(bytes, 0, bytes.Length);
        }
    }
}
