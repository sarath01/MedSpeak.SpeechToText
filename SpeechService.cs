//
// MedSpeak.SpeechToText - SpeechService.cs
// A real-time speech-to-text transcription service using iOS native APIs
// Developed by Sarath Reddy Konda, 2025
//
// Copyright (c) 2025 Sarath Reddy Konda
// Licensed under the MIT License. See LICENSE file in the project root.
//
// This class is intended to help improve patient-provider communication
// in healthcare and accessibility applications.
//


using AVFoundation;
using Foundation;
using Speech;
using System;
using System.Threading.Tasks;

namespace MedSpeak.SpeechToText
{
    public class SpeechService
    {
        private readonly SFSpeechRecognizer speechRecognizer;
        private SFSpeechAudioBufferRecognitionRequest recognitionRequest;
        private SFSpeechRecognitionTask recognitionTask;
        private readonly AVAudioEngine audioEngine;

        public event Action<string> OnTextRecognized;
        public event Action<string> OnError;

        public SpeechService(string locale = "en-US")
        {
            speechRecognizer = new SFSpeechRecognizer(new NSLocale(locale));
            audioEngine = new AVAudioEngine();
        }

        public Task<bool> RequestPermissionsAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            SFSpeechRecognizer.RequestAuthorization((speechStatus) =>
            {
                AVAudioSession.SharedInstance().RequestRecordPermission((micGranted) =>
                {
                    var isAuthorized = (speechStatus == SFSpeechRecognizerAuthorizationStatus.Authorized) && micGranted;
                    tcs.SetResult(isAuthorized);
                });
            });

            return tcs.Task;
        }

        public void StartRecognition()
        {
            try
            {
                StopRecognition();

                var audioSession = AVAudioSession.SharedInstance();
                audioSession.SetCategory(AVAudioSessionCategory.Record);
                audioSession.SetMode(AVAudioSession.ModeMeasurement, out _);
                audioSession.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);

                recognitionRequest = new SFSpeechAudioBufferRecognitionRequest();
                var inputNode = audioEngine.InputNode;
                var recordingFormat = inputNode.GetBusOutputFormat(0);
                inputNode.InstallTapOnBus(0, 1024, recordingFormat, (buffer, _) =>
                {
                    recognitionRequest.Append(buffer);
                });

                audioEngine.Prepare();
                audioEngine.StartAndReturnError(out var error);

                recognitionTask = speechRecognizer.GetRecognitionTask(recognitionRequest, (result, err) =>
                {
                    if (err != null)
                    {
                        OnError?.Invoke(err.LocalizedDescription);
                        return;
                    }

                    if (result != null)
                    {
                        OnTextRecognized?.Invoke(result.BestTranscription.FormattedString);
                    }
                });
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }

        public void StopRecognition()
        {
            if (audioEngine.Running)
            {
                audioEngine.Stop();
                recognitionRequest?.EndAudio();
            }

            recognitionTask?.Cancel();
            recognitionRequest = null;
            recognitionTask = null;

            try
            {
                var inputNode = audioEngine.InputNode;
                inputNode?.RemoveTapOnBus(0);
            }
            catch { }
        }
    }
}
