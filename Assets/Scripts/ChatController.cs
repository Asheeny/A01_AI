using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using OpenAI;
using A01.AgentShared;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System;

namespace A01.Player
{
    public class ChatController : MonoBehaviour
    {
        private static PlayerController player;
        private static string password;

        private readonly string AUDIO_OUTPUT_FILENAME = "output.wav";
        private readonly string AITOKEN_FILENAME = "AIToken";

        [SerializeField] private bool isConversation = true;
        [SerializeField] private bool affectsGameFlow;
        [SerializeField] private GameFlowEventController gameFlowEventController;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendMessageButton;
        [SerializeField] private Button leaveConversationButton;
        [SerializeField] private ScrollRect scrollFieldOutput;
        [SerializeField] private RectTransform sentMessage;
        [SerializeField] private RectTransform receivedMessage;
        [SerializeField] private string promptFileName;
        [SerializeField][Range(0, 1)] float maxTemperature = .2f;

        private float height;

        private string[] possiblePasswords = { "Lobster", "Mushroom", "Wardrobe", "Candle", "Mountain" };
        private string openAI_AccessToken;
        private string correctToken;

        private OpenAIApi openai;
        private InteractionController interactableController;
        private List<ChatMessage> messages = new List<ChatMessage>();

        private string prompt;

        //For Whisper
        [SerializeField] private Button recordVoiceButton;
        [SerializeField] private UnityEngine.UI.Image recordingProgressBar;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private int recordingDuration = 5;

        private AudioClip clip;
        private bool isRecording;
        private float recordingTime;

        private void Awake()
        {
            player = FindFirstObjectByType<PlayerController>();
            interactableController = GetComponentInParent<InteractionController>();
        }

        private void Start()
        {
            openAI_AccessToken = ReadTXTFile(AITOKEN_FILENAME);
            if (password == null)
                password = possiblePasswords[UnityEngine.Random.Range(0, possiblePasswords.Length)];

            SetUpPrompt();
            SetUpUI();

            openai = new OpenAIApi(openAI_AccessToken);

            SendMessage();
        }

        private string GetRandomToken()
        {
            var randomNumber = new byte[16];
            string token = "";

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                token = Convert.ToBase64String(randomNumber);
                token = token.Replace("=", "");
                token = token.Replace("+", "");
            }
            return token;
        }

        //This will setup the prompt with a random password and a token so we know if the player messages fulfill the given requirement i.e. password guessed correctly
        private void SetUpPrompt()
        {
            prompt = ReadTXTFile(promptFileName);
            prompt = prompt.Replace("[PASSWORD]", password);
            prompt = prompt.Replace("[BREAK]", GetRandomToken());

            if (affectsGameFlow)
            {
                correctToken = GetRandomToken();
                prompt = prompt.Replace("[CORRECT]", correctToken);
            }
        }

        private void SetUpUI()
        {
            sendMessageButton.onClick.AddListener(SendMessage);
            leaveConversationButton.onClick.AddListener(EndConversation);

            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }

            recordVoiceButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
        }

        private void Update()
        {
            ManageUI();
        }

        private void ManageUI()
        {
            if (!isRecording)
                return;

            recordingTime += Time.deltaTime;
            recordingProgressBar.fillAmount = recordingTime / recordingDuration;

            if (recordingTime >= recordingDuration)
            {
                recordingTime = 0;
                isRecording = false;
                EndRecording();
            }
        }

        private void EndConversation()
        {
            player.StopInteracting();
            interactableController.SetInteractionState(false);
        }

        private void AppendMessage(ChatMessage message)
        {
            if (!isConversation)
                return;

            scrollFieldOutput.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sentMessage : receivedMessage, scrollFieldOutput.content);
            item.GetComponentInChildren<TextMeshProUGUI>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scrollFieldOutput.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scrollFieldOutput.verticalNormalizedPosition = 0;
        }

        private async void SendMessage()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };

            //If this is the first message, the AI should speak first.
            if (messages.Count == 0)
            {
                newMessage.Content = prompt;
            }
            else
            {
                AppendMessage(newMessage);
            }

            messages.Add(newMessage);

            sendMessageButton.enabled = false;
            recordVoiceButton.enabled = false;
            inputField.text = "";
            inputField.enabled = false;

            // Complete the instruction
            await GetAIResponse();
        }

        private async Task GetAIResponse()
        {
            inputField.text = "Waiting...";

            //Send the request to ChatGPT
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages,
                Temperature = UnityEngine.Random.Range(0, maxTemperature)
            }); ;

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;

                //Has the player succeeded? Or is this just a conversation i.e. no need to check for success
                if (affectsGameFlow && message.Content.Contains(correctToken))
                {
                    message.Content = message.Content.Replace(correctToken, "");
                    EndConversation();
                    gameFlowEventController.TriggerGameFlowEvent(true);
                }

                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Nothing returned from ChatGPT");
            }

            sendMessageButton.enabled = true;
            recordVoiceButton.enabled = true;
            inputField.enabled = true;
            inputField.text = "";

            //Only for non coversation NPC i.e. bridge summoner
            if(!isConversation && messages.Count > 2)
                inputField.text = "Your command did not work...";
        }

        private string ReadTXTFile(string fileName)
        {
            string value;
            string path = Application.dataPath + "/StreamingAssets" + "/" + fileName + ".txt";
            StreamReader reader = new StreamReader(path);
            value = reader.ReadToEnd();
            reader.Close();
            return value;
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        private void StartRecording()
        {
            isRecording = true;
            recordVoiceButton.enabled = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");

            clip = Microphone.Start(dropdown.options[index].text, false, recordingDuration, 44100);
        }

        private async void EndRecording()
        {
            inputField.text = "Transcripting...";

            Microphone.End(null);

            byte[] data = SaveWav.Save(AUDIO_OUTPUT_FILENAME, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                //File = Application.persistentDataPath + "/" + AUDIO_OUTPUT_FILENAME,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            recordingProgressBar.fillAmount = 0;
            inputField.text = res.Text;
            recordVoiceButton.enabled = true;
        }
    }
}