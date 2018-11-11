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
					 patchednsoPath   = new FileInfo(args[2]), 
					 save             = new FileInfo(args[3]);

            using (var patchedstream   = File.OpenRead(patchednsoPath.FullName))
            using (var unpatchedstream = File.OpenRead(unpatchednsoPath.FullName))
            {
                var unpatchedNso = new Nso(unpatchedstream);
                var patchedNso   = new Nso(patchedstream);

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
				
				bool apply = false;
                string patch, offset, fullpatch = string.Empty, fulloffset = string.Empty;
                int combine = 0, lastindex, combinelast;
				
                for (int i = 0; i < unpatchedNso.Sections.Length; i++)
                {
                    NsoSection unpatchedSection = unpatchedNso.Sections[i],
							   patchedSection   = patchedNso.Sections[i];
					
                    byte[] unpatchedData = unpatchedSection.DecompressSection(), 
						   patchedData   = patchedSection.DecompressSection();

                    for (int index = 0; index < patchedData.Length; index++)
                    if (patchedData[index] != unpatchedData[index])
                    {
                        offset = $"{index:X8}";
                        patch = $"{patchedData[index]:X2}";

                        //get the last index
                        lastindex = index - 1;

                        if (!apply && index - 1 == lastindex)
                        {
                            fullpatch = string.Empty;
                            fulloffset = offset;
                            apply = true;
                        }

                        if (index - 1 == lastindex)
                        {
                            fullpatch += patch;
                            combine = combine + 1; 
                        }

                        if (!apply && combine == 0) patcharraylist.Add($"{offset} {patch}");

                        //check if apply is true and index is lastindex
                        combinelast = 1 - combine;
						
                        if (apply && combine > combinelast)
                        {
                            //apply combined offset & patch and reset
                            patcharraylist.Add($"{fulloffset} {fullpatch}");
                            apply = false;
                            fullpatch = "";
                            fulloffset = "";
                            combine = 0;
                        }
                    }
                }
				
                File.WriteAllLines(save.FullName, patcharraylist);
            }
        }
    }
}
