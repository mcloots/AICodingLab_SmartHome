using Microsoft.CognitiveServices.Speech;

namespace SmartHome
{

    public class SpeechService
    {
        private string _key;
        private SpeechRecognizer _speechRecognizer;

        public SpeechService(string key)
        {
            _key = key;  
            
            var config = SpeechConfig.FromSubscription(_key, "westeurope");
            config.SpeechRecognitionLanguage = "nl-BE";

            _speechRecognizer = new SpeechRecognizer(config);  
        }

        public async Task<string> ReadTextAsync()
        {
            SpeechRecognitionResult result; 

            do 
            { 
                result = await _speechRecognizer.RecognizeOnceAsync(); 
            } while (result.Reason != ResultReason.RecognizedSpeech); 
            
            return result.Text.Trim();
        }
    }
}
