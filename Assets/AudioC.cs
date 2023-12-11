using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class AudioC: MonoBehaviour
{

    const string SAMPLING_ON = "Sampling_ON";
    const string SAMPLING_OFF = "Sampling_OFF";
    const string NOTIFY_ON = "ON";
    const string NOTIFY_OFF = "OFF";
    const string applyURL = "/webhookURL/";


    // ���g���̕���\��ݒ�
    const int SPECTRUM_SIZE = 512;

    NoteNameDetector nnd;
    public float timer;
    public GameObject p_list;
    Dropdown d_down;
    public Button muteButton;
    // �T���v�����O�̃I���I�t
    bool isSampling = false;
    public GameObject _spctrumbar;
    public List<GameObject> spectrumBarList;
    public GameObject spectrumBars;
    float barwidth = Screen.width / SPECTRUM_SIZE * 4;
    public GameObject panelfit;
    
    // �ݒ�p�p�l��
    public GameObject settingPanel;
    public static int threshold = 0;
    public int th_mimrror;

    // �ݒ蒆���ǂ���
    bool isSetting = false;

    // �ʒm���邩�ǂ���
    bool isNotifying = false;
    public Button notifySettingButton;
    public float th;
    public Text resultText;

    // ���x�ݒ�
    public float threashVolume;
    public Text volumeText;

    // 臒l
    int targetTone;
    int targetVal;

    // ��ʂɉ��K��\������Text
    public Text toneText;
    // ���ʂɕ␳��������
    public float herit = 400;
    // �ő剹��T��
    public float MaxTone = -1;
    public float MaxToneValue = -1;
    AudioSource aud;

    // ����������ȏ�ɂȂ����o�[�͖�������
    public int dontCareIndex = 999;

    // Start is called before the first frame update
    void Start()
    {
        dontCareIndex = Mathf.FloorToInt((Screen.width + barwidth) / barwidth);

        timer = 0;
        nnd = new NoteNameDetector();
        aud = GetComponent<AudioSource>();
        // �}�C�N���A���[�v���邩�ǂ����AAudioClip�̕b���A�T���v�����O���[�g ���w�肷��
        aud.clip = Microphone.Start(null, true, 1 , 44100);
        aud.mute = true;
        aud.Play();
        foreach(string device in Microphone.devices)
        {
            Debug.Log(device);
        }
        d_down = p_list.GetComponent<Dropdown>();
        d_down.ClearOptions();
        List<string> devicesList = new List<string>();
        devicesList.AddRange(Microphone.devices);
        d_down.AddOptions(devicesList);

        muteButton.transform.GetChild(0).GetComponent<Text>().text = SAMPLING_OFF;

        for(int i = 0; i < dontCareIndex; i++)
        {
            GameObject bar = Instantiate(_spctrumbar, spectrumBars.GetComponent<RectTransform>());
            bar.GetComponent<RectTransform>().position = new Vector3(barwidth * i, 0, 0);
            bar.GetComponent<RectTransform>().sizeDelta =new Vector2(barwidth,0);
            spectrumBarList.Add(bar);
            bar.name = "tone_" + i.ToString("D3");

        }

        settingPanel.SetActive(false);
        panelfit.SetActive(false);
        notifySettingButton.transform.GetChild(0).GetComponent<Text>().text = isNotifying ? "ON" : "OFF";

        // �ۑ������f�[�^�̓ǂݏo��
        LoadPrefs();
        volumeText.text = threashVolume.ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {
        if (isSampling)
        {
            float[] spectrum = new float[SPECTRUM_SIZE];
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            var maxIndex = 0;
            var maxValue = 0.0f;
            for (int i = 0; i < dontCareIndex; i++)
            {
                var val = spectrum[i];
                if (val > maxValue)
                {
                    maxValue = val;
                    maxIndex = i;
                }
                spectrumBarList[i].GetComponent<RectTransform>().sizeDelta = new Vector2(barwidth, val * herit);
            }
            var freq = maxIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length;
            if (maxValue != 0)
            {
                toneText.text = string.Format("{0:000.000}", 0.1 * maxIndex * aud.clip.frequency / 2 / SPECTRUM_SIZE);
                //toneText.text = nnd.GetNoteName(freq);

                if (maxValue > MaxToneValue)
                {
                    MaxToneValue = maxValue;
                    MaxTone = maxIndex;
                }
            }
            if (isNotifying)
            {
                th = spectrum[threshold];
            }
            if (isNotifying && spectrum[threshold] > threashVolume && timer == 0)
            {
                panelfit.SetActive(true);
                StartCoroutine("GET");
                timer = 5;
            }
            if (timer <= 0)
            {
                panelfit.SetActive(false);
                timer = 0;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
        th_mimrror = threshold;
        volumeText.text = threashVolume.ToString("F2");
    }

    public void startSampling()
    {
        Debug.Log(muteButton.transform.GetChild(0).GetComponent<Text>().text);
        if(isSampling) // �T���v�����O���Ȃ��~
        {
            
        }
        else
        {
            
        }
        // ���]������
        isSampling = !isSampling;
        
        aud.mute = !isSampling;
        muteButton.transform.GetChild(0).GetComponent<Text>().text = isSampling ? SAMPLING_ON : SAMPLING_OFF;
    }


    public void onClick_Setting()
    {
        // �ݒ�J�n
        if (!isSetting)
        {
            Debug.Log("StartSetting");
            // �T���v�����O���~
            isSampling = false;
            muteButton.transform.GetChild(0).GetComponent<Text>().text = SAMPLING_OFF;
            // �T���v�����O�{�^�����\����
            muteButton.transform.gameObject.SetActive(false);

            // �o�[���ꂼ��̍�����50��
            foreach(GameObject bar in spectrumBarList)
            {
                bar.GetComponent<RectTransform>().sizeDelta = new Vector2(barwidth, 50);
            }
            
        }
        else
        {
            muteButton.transform.gameObject.SetActive(true);
            // �o�[���ꂼ��̍�����0��
            foreach (GameObject bar in spectrumBarList)
            {
                bar.GetComponent<RectTransform>().sizeDelta = new Vector2(barwidth, 0);
            }
        }
        settingPanel.SetActive(!isSetting);

        isSetting = !isSetting;
    }

    public void onClick_Setting_NotBarSet(int value)
    {
        threshold += value;
        if (threshold > dontCareIndex)
        {
            threshold = dontCareIndex - 1;
            return;
        }
        if (threshold < 0)
        {
            threshold = 0;
            return;
        }
        spectrumBarList[threshold].GetComponent<Image>().color = Color.green;
        spectrumBarList[threshold-value].GetComponent<Image>().color = new Color(1 ,1 ,1, 100 / 255f);

    }

    public void onClick_Setting_Volume(float value)
    {
        threashVolume += value;
        
        if (threashVolume < 0)
        {
            threshold = 0;
            return;
        }else if(threashVolume > 1)
        {
            threashVolume = 1;
        }
    }

    public void onClick_Notifi()
    {
        isNotifying = !isNotifying;
        notifySettingButton.transform.GetChild(0).GetComponent<Text>().text = isNotifying ? "ON" : "OFF";
    }

    private IEnumerator GET()
    {
        using (var req = UnityWebRequest.Get(applyURL))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                resultText.text = req.error;
            }
            else if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                resultText.text = req.error;
            }
            else
            {
                resultText.text = req.downloadHandler.text;
            }
        }
    }

    // �ۑ��f�[�^�̕ۑ�
    public void SavePrefs()
    {
        PlayerPrefs.SetInt("THREASH_HOLD", threshold);
        PlayerPrefs.SetFloat("THREASH_VOL", threashVolume);
        PlayerPrefs.Save();

    }

    // �ۑ��f�[�^�̓ǂݏo��
    public void LoadPrefs()
    {
        threshold = PlayerPrefs.GetInt("THREASH_HOLD", 0);
        threashVolume = PlayerPrefs.GetFloat("THREASH_VOL", 1F);
    }

    private void OnApplicationQuit()
    {
        SavePrefs();
    }
}
