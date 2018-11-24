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
                return;
            }

            string patchname = args[0];

            FileInfo unpatchednsoPath = new FileInfo(args[1]),
                     patchednsoPath = new FileInfo(args[2]),
                     save = new FileInfo(args[3]);

            using (var patchedstream = File.OpenRead(patchednsoPath.FullName))
            using (var unpatchedstream = File.OpenRead(unpatchednsoPath.FullName))
            {
                var unpatchedNso = new Nso(unpatchedstream);
                var patchedNso = new Nso(patchedstream);

                if (patchedNso.Sections.Length != unpatchedNso.Sections.Length)
                {
                    Console.WriteLine("The NSO section lengths do not match (NSO comes from a diffrent application?)");
                    return;
                }

                List<string> patcharraylist = new List<string>
                {
                    $"//{patchname}",
                    "@enabled"
                };

                string patch, offset = string.Empty;

                for (int i = 0; i < unpatchedNso.Sections.Length; i++)
                {
                    NsoSection unpatchedSection = unpatchedNso.Sections[i],
                               patchedSection = patchedNso.Sections[i];

                    byte[] unpatchedData = unpatchedSection.DecompressSection(),
                           patchedData = patchedSection.DecompressSection();

                    for (int index = 0; index < patchedData.Length; index++)
                        if (patchedData[index] != unpatchedData[index])
                        {
                            offset = $"{index:X8}";
                            patch = $"{patchedData[index]:X2}";
                            patcharraylist.Add($"{offset} {patch}");
                        }
                        else
                        {
                            Console.WriteLine("No diffrences found.");
                            break;
                        }
                }
                File.WriteAllLines(save.FullName, patcharraylist);
                Console.WriteLine($"Done, {patcharraylist.Count - 2} were different.");
            }
        }
    }
}
