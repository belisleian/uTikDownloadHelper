using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace uTikDownloadHelper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (Common.Settings.ticketWebsite == null) Common.Settings.ticketWebsite = "";

            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            if (args.Length == 0)
            {
                using (frmList frm = new frmList())
                {
                    frm.Show();
                    Application.Run();
                }

                return;
            }
            
            string ticketInputPath = args[0];

            Byte[] ticket = { };

            if (ticketInputPath.ToLower().StartsWith("http"))
            {
                try
                {
                    ticket = (new System.Net.WebClient()).DownloadData(ticketInputPath);
                } catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "Error Downloading Ticket");
                    return;
                }
            } else
            {
                ticket = File.ReadAllBytes(ticketInputPath);
            }

            if (ticket.Length < 0x1DC)
            {
                MessageBox.Show("Invalid Ticket");
            }

            patchTicket(ref ticket);

            string hexID = "";

            for (int i = 0x1DC; i < 0x1DC + 8; i++)
            {
                hexID += string.Format("{0:X2}", ticket[i]);
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the folder to download the files to.";
            fbd.SelectedPath = Common.Settings.lastPath;

            ChooseFolder:
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Common.Settings.lastPath = System.IO.Directory.GetParent(fbd.SelectedPath).FullName;

                int directoryItems = 0;
                directoryItems += Directory.GetFiles(fbd.SelectedPath).Length;
                directoryItems += Directory.GetDirectories(fbd.SelectedPath).Length;

                string nusGrabberPath = Path.Combine(fbd.SelectedPath, "NUSgrabber.exe");
                string wgetPath = Path.Combine(fbd.SelectedPath, "wget.exe");
                string vcRuntime140Path = Path.Combine(fbd.SelectedPath, "vcruntime140.dll");

                if (directoryItems == 0)
                {
                    File.WriteAllBytes(nusGrabberPath, Properties.Resources.NUSgrabber);
                    File.WriteAllBytes(wgetPath, Properties.Resources.wget);
                    File.WriteAllBytes(vcRuntime140Path, Properties.Resources.vcruntime140);

                    string downloadPath = Path.Combine(fbd.SelectedPath, hexID);

                    if (!Directory.Exists(downloadPath)) Directory.CreateDirectory(downloadPath);

                    File.Create(Path.Combine(downloadPath, "cetk")).Close();

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

                    if (!File.Exists(Path.Combine(downloadPath, "title.tik")))
                    {
                        // There was some kind of error with NUSgrabber...
                        Directory.Delete(downloadPath, true);
                        MessageBox.Show("There was an error downloading title ID " + hexID.ToUpper(), "Error Downloading", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    foreach (string file in Directory.GetFiles(downloadPath))
                    {
                        string name = Path.GetFileName(file);

                        File.Move(file, Path.Combine(fbd.SelectedPath, name));
                    }
                    Directory.Delete(downloadPath);

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

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs e)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            // Get the Name of the AssemblyFile
            var assemblyName = new AssemblyName(e.Name);
            var dllName = assemblyName.Name + ".dll";

            // Load from Embedded Resources - This function is not called if the Assembly is already
            // in the same folder as the app.
            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(dllName));
            if (resources.Any())
            {

                // 99% of cases will only have one matching item, but if you don't,
                // you will have to change the logic to handle those cases.
                var resourceName = resources.First();
                using (var stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    var block = new byte[stream.Length];

                    // Safely try to load the assembly.
                    try
                    {
                        stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }
                    catch (IOException)
                    {
                        return null;
                    }
                    catch (BadImageFormatException)
                    {
                        return null;
                    }
                }
            }

            // in the case the resource doesn't exist, return null.
            return null;
        }
    }
}
