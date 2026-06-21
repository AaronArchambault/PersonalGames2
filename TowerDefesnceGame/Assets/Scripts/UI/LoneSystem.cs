using UnityEngine;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class Loan
{
    public int   totalOwed;
    public int   wavesRemaining;
    public int   repaymentPerWave;
}

public class LoanSystem : MonoBehaviour
{
    public static LoanSystem Instance { get; private set; }

    [Header("Loan Settings")]
    public int   loanAmount     = 100;
    public float interestRate   = 0.3f;  // 30% interest
    public int   repaymentWaves = 3;
    public int   maxLoans       = 2;     // can't take too many at once

    [Header("UI")]
    public GameObject loanPanel;
    public TextMeshProUGUI loanStatusText;
    public UnityEngine.UI.Button takeLoanButton;

    private List<Loan> activeLoans = new();
    private int totalOwed => GetTotalOwed();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += ProcessRepayments;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= ProcessRepayments;
    }

    public bool TakeLoan()
    {
        if (activeLoans.Count >= maxLoans)
        {
            UIManager.Instance?.Announce("Too many loans!", Color.red);
            return false;
        }

        int repayTotal = Mathf.RoundToInt(loanAmount * (1f + interestRate));
        int perWave    = Mathf.CeilToInt((float)repayTotal / repaymentWaves);

        activeLoans.Add(new Loan
        {
            totalOwed        = repayTotal,
            wavesRemaining   = repaymentWaves,
            repaymentPerWave = perWave
        });

        GameManager.Instance.EarnGold(loanAmount);
        FloatingTextPool.Instance?.Spawn(
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)),
            $"LOAN: +{loanAmount}g (owe {repayTotal}g)", Color.cyan);

        UpdateUI();
        return true;
    }

    void ProcessRepayments()
    {
        var toRemove = new List<Loan>();
        foreach (var loan in activeLoans)
        {
            int payment = Mathf.Min(loan.repaymentPerWave, GameManager.Instance.Gold);
            GameManager.Instance.SpendGold(payment);
            loan.totalOwed      -= payment;
            loan.wavesRemaining -= 1;

            FloatingTextPool.Instance?.Spawn(
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.6f, 10f)),
                $"LOAN REPAYMENT: -{payment}g", Color.red);

            if (loan.wavesRemaining <= 0 || loan.totalOwed <= 0)
                toRemove.Add(loan);
        }
        foreach (var loan in toRemove) activeLoans.Remove(loan);
        UpdateUI();
    }

    int GetTotalOwed()
    {
        int total = 0;
        foreach (var l in activeLoans) total += l.totalOwed;
        return total;
    }

    void UpdateUI()
    {
        if (loanStatusText)
        {
            loanStatusText.text = activeLoans.Count > 0
                ? $"Loans: {activeLoans.Count}  Owed: {totalOwed}g"
                : "No active loans";
        }
        if (takeLoanButton)
            takeLoanButton.interactable = activeLoans.Count < maxLoans;
    }

    public void OnTakeLoanButton() => TakeLoan();
}