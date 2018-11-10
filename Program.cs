using LibHac;
using System;
using System.Collections.Generic;
using System.IO;
using static LibHac.Nso;

namespace IPSwitch_PatchMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: IPSwitch-PatchMaker.exe [name of patch] [unpatched NSO] [patched NSO] [save location]");
            }

            string patchname = args[0];
            FileInfo unpatchednsoPath = new FileInfo(args[1]);
            FileInfo patchednsoPath = new FileInfo(args[2]);
            FileInfo save = new FileInfo(args[3]);

            using (FileStream patchedstream = new FileStream(patchednsoPath.FullName, FileMode.Open))
            using (FileStream unpatchedstream = new FileStream(unpatchednsoPath.FullName, FileMode.Open))
            {
                Nso unpatchedNso = new Nso(unpatchedstream);
                Nso patchedNso = new Nso(patchedstream);


                if (patchedNso.Sections.Length != unpatchedNso.Sections.Length)
                {
                    Console.WriteLine("The NSO section Lengths do not match (NSO comes from a diffrent application?)");
                    return;
                }

                List<string> patcharraylist = new List<string>
                {
                    $"//{patchname}",
                    "@enabled"
                };
                for (int i = 0; i < unpatchedNso.Sections.Length; i++)
                {
                    NsoSection unpatchedSection = unpatchedNso.Sections[i];
                    NsoSection patchedSection = patchedNso.Sections[i];
                    byte[] unpatchedData = unpatchedSection.DecompressSection();
                    byte[] patchedData = patchedSection.DecompressSection();

                    for (int index = 0; index < patchedData.Length; index++)
                        if (patchedData[index] != unpatchedData[index])
                        {
                            patcharraylist.Add(string.Format("{0:X8} {1:X2}", index, patchedData[index]));
                        }
                }
                File.WriteAllLines(save.FullName, patcharraylist);
            }
        }
    }
}
