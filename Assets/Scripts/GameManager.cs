using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Numerics;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    [Header("DICE")]
    [SerializeField] GameObject dicePrefab;
    [SerializeField] Transform diceSpawnPosition;
    private GameObject dice;

    private bool enableCheckSides;
    private string myPickEvenOrOdd;

    public BigInteger tokenBalance;
    public BigInteger depositAmount;
    public BigInteger playerBalance;
    public BigInteger winAmount;
    public int streak = 0;

    [Header("UI")]
    [SerializeField] Button evenButton;
    [SerializeField] Button oddButton;
    [SerializeField] public Button depositButton;
    [SerializeField] Button withdrawButton;
    [SerializeField] public Text playerBalanceText;
    [SerializeField] Text barText;
    [SerializeField] Slider bar;
    [SerializeField] Transform winPopUp;
    [SerializeField] Transform losePopUp;
    [SerializeField] Text winAmountText;
    [SerializeField] GameObject fader;
    [SerializeField] GameObject inputSection;
    [SerializeField] Text depositText;
    [SerializeField] GameObject errorMessage;
    [SerializeField] GameObject refreshSection;
    [SerializeField] GameObject settingsPopUp;
    [SerializeField] List<AudioSource> sfx = new List<AudioSource>();
    [SerializeField] List<AudioSource> music = new List<AudioSource>();
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;


    private void Awake()
    {
        instance = this;   
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("music"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("music");
            for (int i = 0; i < music.Count; i++)
                music[i].volume = PlayerPrefs.GetFloat("music");
        }
        if (PlayerPrefs.HasKey("sfx"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("sfx");
            for (int i = 0; i < sfx.Count; i++)
                sfx[i].volume = PlayerPrefs.GetFloat("sfx");
        }
    }

    public void EvenButton()
    {
        DisableUIInteractions();
        myPickEvenOrOdd = "Even";
        RollTheDice();
        Invoke("EnableRefresh", 20);
    }

    public void OddButton()
    {
        DisableUIInteractions();
        myPickEvenOrOdd = "Odd";
        RollTheDice();
        Invoke("EnableRefresh", 20);
    }

    private void EnableRefresh()
    {
        refreshSection.GetComponent<CanvasGroup>().DOFade(0, 0).OnComplete(delegate ()
        {
            refreshSection.SetActive(true);
            refreshSection.GetComponent<CanvasGroup>().DOFade(1, 0.5f).OnComplete(delegate ()
            {
               
            });
        });
    }

    public void RefreshClick()
    {
        refreshSection.SetActive(false);
        if(dice != null)
            Destroy(dice);
        EnableUIInteractions();
    }

    public void DisableUIInteractions()
    {
        oddButton.interactable = false;
        evenButton.interactable = false;
        depositButton.interactable = false;
        withdrawButton.interactable = false;
    }

    public void EnableUIInteractions()
    {
        oddButton.interactable = true;
        evenButton.interactable = true;
        //depositButton.interactable = true;
        //withdrawButton.interactable = true;
    }

    public void CheckEvenOrOdd(int number, int temp)
    {
        CancelInvoke();
        refreshSection.SetActive(false);
        string result;

        if (number % 2 == 0)
            result = "Even";
        else
            result = "Odd";

        //print(result);

        if (result.Equals(myPickEvenOrOdd))
            WinPopUp(Random.Range(1, 1000), Random.Range(1, 1000));
        else
            LosePopUp();
    }

    private void WinPopUp(int temp, int temp1)
    {
        withdrawButton.interactable = true;

        streak++;
        winAmount *= 2;

        winAmountText.text = winAmount / 2 + "";
        winPopUp.gameObject.SetActive(true);

        winPopUp.transform.DOLocalMoveY(90, 1f, false).OnComplete(delegate ()
        {
            winPopUp.transform.DOLocalMoveY(820, 0.5f, false).SetDelay(2).OnComplete(delegate ()
            {
                Destroy(dice);
                AddBalance(winAmount);
                winPopUp.gameObject.SetActive(false);
            });
        });
    }

    private void LosePopUp()
    {
        losePopUp.gameObject.SetActive(true);

        losePopUp.transform.DOLocalMoveY(90, 1f, false).OnComplete(delegate ()
        {
            losePopUp.transform.DOLocalMoveY(820, 0.5f, false).SetDelay(2).OnComplete(delegate ()
            {
                BetLost();
                losePopUp.gameObject.SetActive(false);
            });
        });
    }

    private void AddBalance(BigInteger winAmount)
    {
        playerBalance = winAmount;
        playerBalanceText.text = playerBalance.ToString();
        AdjustBar();
    }

    private void BetLost()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Withdraw()
    {
        Transaction.instance.Winner();
    }

    public void Deposit()
    {
        DisableUIInteractions();
        Transaction.instance.GetBalance();
    }

    public void DisplayDepositSection()
    {
        fader.transform.GetComponent<Image>().DOFade(0, 0).OnComplete(delegate ()
        {
            fader.gameObject.SetActive(true);
            fader.transform.GetComponent<Image>().DOFade(0.9f, 0.5f).OnComplete(delegate ()
            {
                inputSection.SetActive(true);
                inputSection.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
            });
        });
    }

    public void ConfirmDeposit()
    {
        if (BigInteger.Parse(depositText.text) > tokenBalance)
        {
            errorMessage.GetComponent<Text>().text = "Error: You do not have sufficient $DICE tokens in your wallet";
            errorMessage.SetActive(true);
            errorMessage.GetComponent<Animator>().Play("Error", -1, 0);
            return;
        }

        else if (BigInteger.Parse(depositText.text) > 0 && BigInteger.Parse(depositText.text) <= 15000000)
        {
            depositAmount = BigInteger.Parse(depositText.text);

            //GameManager.instance.playerBalance += GameManager.instance.depositAmount;
            //GameManager.instance.playerBalanceText.text = GameManager.instance.playerBalance.ToString();
            //winAmount = depositAmount;
            //EnableUIInteractions();

            inputSection.SetActive(false);
            fader.SetActive(false);
            Transaction.instance.Approve();
        }

        else
        {
            errorMessage.GetComponent<Text>().text = "Error: The wager should be between 0 - 15,000,000 $DICE";
            errorMessage.SetActive(true);
            errorMessage.GetComponent<Animator>().Play("Error", -1, 0);
        }
    }

    public void OnInputValueChanged()
    {
        errorMessage.SetActive(false);
    }

    public void AdjustBar()
    {
        if (bar.value < 1)
        {
            bar.DOValue(bar.value + 0.1f, 0.7f).OnComplete(delegate() {
                EnableUIInteractions();
            });
        }       
        barText.text = Mathf.Pow(2, streak).ToString() + "x";
    }


    private void FixedUpdate()
    {
        if (dice != null && !enableCheckSides)
        {
            if (dice.GetComponent<Rigidbody>().velocity == UnityEngine.Vector3.zero)
            {
                enableCheckSides = true;
                dice.GetComponent<CheckSide>().enabled = true;
            }
        }
    }

    public void RollTheDice()
    {
        if (dice != null)
            Destroy(dice);

        dice = Instantiate(dicePrefab, diceSpawnPosition.position, UnityEngine.Quaternion.Euler(0, Random.Range(-40, 40), 0));
        dice.GetComponent<AudioSource>().volume = sfxSlider.value;
        enableCheckSides = false;

        float dirX = Random.Range(12000, 15000);
        float dirY = Random.Range(12000, 15000);
        float dirZ = Random.Range(12000, 15000);

        dice.GetComponent<Rigidbody>().AddForce(dice.transform.forward * 500, ForceMode.Impulse);
        dice.GetComponent<Rigidbody>().AddTorque(dirX, dirY, dirZ);
    }


    public void SettingsButton()
    {
        settingsPopUp.SetActive(true);
    }

    public void SettingsCloseButton()
    {
        settingsPopUp.SetActive(false);
    }

    public void OnMusicValueChanged()
    {
        for (int i = 0; i < music.Count; i++)
            music[i].volume = musicSlider.value;
        PlayerPrefs.SetFloat("music", musicSlider.value);
    }

    public void OnSfxValueChanged()
    {
        for (int i = 0; i < sfx.Count; i++)
            sfx[i].volume = sfxSlider.value;
        PlayerPrefs.SetFloat("sfx", sfxSlider.value);
    }
}
