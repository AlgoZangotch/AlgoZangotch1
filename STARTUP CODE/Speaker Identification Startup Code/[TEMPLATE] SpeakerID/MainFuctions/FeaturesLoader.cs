using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NAudio.Midi;
using Recorder;
using Recorder.MFCC;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;




namespace Recorder
{

    public struct UserStruct
    {

        public String Username;
        public List<Sequence> Sequences;

    }
    public static class FeaturesLoader
    {

        public static List<UserStruct> LoadSequencesFromBinaryFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File not found: " + path);
                return new List<UserStruct>();
            }

            string json = File.ReadAllText(path);
            List<UserStruct> sequences = JsonConvert.DeserializeObject<List<UserStruct>>(json);

            return sequences;
        }


        public static string TrainingFeaturesSaver(List<User> users, String testcase, int number)
        {
            List<UserStruct> userStruct = new List<UserStruct>();

            for (int i = 0; i < users.Count; i++)
            {
                User x = users[i];
                List<Sequence> sequences = new List<Sequence>();

                for (int j = 0; j < x.UserTemplates.Count; j++)
                {
                    AudioSignal cleanedSig = AudioOperations.RemoveSilence(x.UserTemplates[j]);
                    sequences.Add(AudioOperations.ExtractFeatures(cleanedSig));
                }

                UserStruct temp = new UserStruct
                {
                    Username = x.UserName,
                    Sequences = sequences
                };

                userStruct.Add(temp);
            }

            string currentDir = Directory.GetCurrentDirectory();
            string parentDir = Directory.GetParent(currentDir).FullName;
            string saveDir = Path.Combine(parentDir, testcase, number.ToString());

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            string saveFilePath = Path.Combine(saveDir, "training_sequences.json");

            if (File.Exists(saveFilePath))
            {
                Console.WriteLine("File already exists. Skipping save.");
                return saveFilePath;
            }

            string json = JsonConvert.SerializeObject(userStruct, Formatting.Indented);
            File.WriteAllText(saveFilePath, json);

            Console.WriteLine($"Sequences saved to: {saveFilePath}");
            return saveFilePath;
        }



        public static List<UserStruct> testFeature(List<User> users)
        {
            List<UserStruct> userStruct = new List<UserStruct>();

            for (int i = 0; i < users.Count; i++)
            {
                User x = users[i];
                List<Sequence> sequences = new List<Sequence>();

                for (int j = 0; j < x.UserTemplates.Count; j++)
                {
                    AudioSignal cleanedSig = AudioOperations.RemoveSilence(x.UserTemplates[j]);
                    sequences.Add(AudioOperations.ExtractFeatures(cleanedSig));
                }

                UserStruct temp = new UserStruct
                {
                    Username = x.UserName,
                    Sequences = sequences
                };

                userStruct.Add(temp);
            }

            return userStruct;
        }

    }
}