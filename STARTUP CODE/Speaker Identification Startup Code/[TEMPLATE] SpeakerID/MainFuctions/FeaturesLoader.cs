using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recorder;
using Recorder.MFCC;

namespace Recorder
{
    public static class FeaturesLoader
    {
        public static List<Sequence> TrainingFeaturesLoader(string path)
        {
            List<Sequence> seqs = new List<Sequence>();


            string[] audioFiles = Directory.GetFiles(path, "*.wav");

            foreach (string audioFile in audioFiles)
            {

                AudioSignal audioSignal = new AudioSignal();

                audioSignal = AudioOperations.OpenAudioFile(audioFile);

                AudioSignal audioSignalCleansed = AudioOperations.RemoveSilence(audioSignal);

                Sequence seq = AudioOperations.ExtractFeatures(audioSignalCleansed);

                seqs.Add(seq);
            }
            
            return seqs;

        }

        public static Sequence testFeatureLoader(string path)
        {
            string audioFile = Directory.EnumerateFiles(path, "*.wav").FirstOrDefault();

            AudioSignal audioSignal = new AudioSignal();

            audioSignal = AudioOperations.OpenAudioFile(audioFile);

            AudioSignal audioSignalCleansed = AudioOperations.RemoveSilence(audioSignal);

            Sequence seq = AudioOperations.ExtractFeatures(audioSignalCleansed);

            return seq;

        }


    }
}