# MedSpeak.SpeechToText

A speech-to-text .NET for iOS library by Sarath Konda (2025), designed to help patients and healthcare providers communicate more effectively.

## Key Features
- Real-time transcription via Apple SFSpeechRecognizer
- Localized input (e.g., "en-US", "es-ES")
- Designed for healthcare, accessibility, and elder care

## Example Usage

```csharp
var speechService = new SpeechService("en-US");
speechService.OnTextRecognized += (text) => Console.WriteLine(text);
speechService.OnError += (err) => Console.WriteLine($"Error: {err}");

await speechService.RequestPermissionsAsync();
speechService.StartRecognition();
```

## Licensing

This project is open-sourced under the MIT license by Sarath Konda in support of EB2-NIW.
See the LICENSE file for details.
