using System;
using SpaceMath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Sub managers
    [Header("Managers")] [SerializeField] private AttackManager _am;
    [SerializeField] private GameBoardManager _gbm;
    [SerializeField] private UIManager _uim;
    [SerializeField] private SelectionGridManager _sgm;
    [SerializeField] private WatsonManager _wm;
    [SerializeField] private MessageManager _mm;
    [SerializeField] private ShieldManager _sm;
    [SerializeField] private inGameMenu _Igm;
    [SerializeField] private Scores _scores;

    private Ships _player;
    private List<Ships> _enemies;

    // Store the game stage.
    private enum Stages
    {
        None,
        Question,
        Player,
        Enemies
    };

    private Stages _stage;    

    // Drag the canvas into these variables in the inspector
    [Header("Canvas")] [SerializeField] private GameObject _questionCanvas;
    [SerializeField] private GameObject _selectionCanvas;
    [SerializeField] private GameObject _messageCanvas;

    // Game settings
    [SerializeField] private GameObject _restartButton;
    [Header("Misc")] [SerializeField] private float _panelHigh;

    private void Start()
    {
        // Question stage
        _stage = Stages.Question;
        _player = _gbm.GetPlayer();        

        SetPanel(PanelType.Question);
    }

    private void Update()
    {
        if (_player.IsShipDead())
        {
            ShowLoseScreen();            
            _scores.checkScore(_player.Energy);
        }
        else if (!_gbm.IsEnemiesRemain())
        {
            ShowWinScreen();            
            _scores.checkScore(_player.Energy);
        }
        else
            switch (_stage)
            {
                // Question stage
                case Stages.Question:
                    SetPanel(PanelType.Question);
                    switch (_uim.GetAnswerState())
                    {
                        // Answer was correct
                        case UIManager.AnswerStates.Right:
                            _stage = Stages.None;
                            _uim.SetAnswerState(UIManager.AnswerStates.Suspension);
                            StartCoroutine(QuestionToPlayerTurn());
                            _scores.incrementScore();
                            break;

                        // Answer was incorrect
                        case UIManager.AnswerStates.Wrong:
                            _stage = Stages.None;
                            _uim.SetAnswerState(UIManager.AnswerStates.Suspension);
                            StartCoroutine(QuestionToEnemiesTurn());
                            break;
                    }

                    break;

                // Player's turn
                case Stages.Player:
                    SetPanel(PanelType.Selection);
                    // check Watson running
                    if (_wm.IsWatsonRunning())
                    {
                        _stage = Stages.None;
                        StartCoroutine(RunWatsonCommand());
                        break;
                    }

                    if (_sm.IsClicked)
                    {
                        _stage = Stages.None;
                        StartCoroutine(SwitchShield(_player));
                        break;
                    }

                    var selectionResult = _sgm.GetFinalResult();
                    _sgm.ResetFinalResult();
                    if (selectionResult != null && selectionResult.Type != ActionType.None)
                    {
                        _stage = Stages.None;
                        switch (selectionResult.Type)
                        {
                            case ActionType.Move:
                                StartCoroutine(Move(_player, selectionResult.TargetIndex.x,
                                    selectionResult.TargetIndex.y));
                                break;
                            case ActionType.Attack:
                                StartCoroutine(AttackEnemy(_player, _gbm.GetShip(selectionResult.TargetIndex)));
                                break;
                        }
                    }

                    break;

                // Enemies' turn
                case Stages.Enemies:
                    _stage = Stages.None;
                    _enemies = _gbm.GetEnemiesInRange();
                    StartCoroutine(AttackPlayer(_enemies, _player));
                    break;
                default:
                    // reset the second selection input occurred in none stage.
                    _sgm.ResetFinalResult();
                    break;
            }
    }

    private void SetPanel(PanelType type)
    {
        var targetPanel = type switch
        {
            PanelType.Question => _questionCanvas,
            PanelType.Selection => _selectionCanvas,
            _ => _messageCanvas
        };
        var panels = new HashSet<GameObject>() { _questionCanvas, _selectionCanvas, _messageCanvas };
        panels.Remove(targetPanel);
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
        if(!targetPanel.activeSelf)
            targetPanel.SetActive(true);
        SetPanelPosition();
    }

    private void ShowMessage(string message, Stages nextStage)
    {
        _mm.SetMessage(message);
        SetPanel(PanelType.Message);
        _mm.SetCloseAction(() => { _stage = nextStage; });
    }

    private IEnumerator QuestionToPlayerTurn()
    {
        yield return new WaitForSeconds(0.8f);
        SetPanel(PanelType.Selection);
        _stage = Stages.Player;
    }

    private IEnumerator QuestionToEnemiesTurn()
    {
        yield return new WaitForSeconds(0.8f);
        _questionCanvas.SetActive(false);
        _selectionCanvas.SetActive(false);
        _stage = Stages.Enemies;
    }

    private IEnumerator AttackEnemy(Ships player, Ships enemy)
    {
        yield return new WaitForSeconds(0.5f);
        _am.Attack(player, enemy);
        yield return new WaitForSeconds(1.0f);
        _stage = Stages.Enemies;
        _sgm.UpdateSelectionUI();
    }

    private IEnumerator AttackPlayer(List<Ships> enemies, Ships player)
    {
        if (enemies != null)
            foreach (var t in enemies)
            {
                _am.Attack(t, player);
                yield return new WaitForSeconds(1.0f);
            }
        else
            yield return new WaitForSeconds(1.0f);

        // Always consume player's energy at each turn's ending
        player.ConsumeEnergyByTurn();
        _stage = Stages.Question;
        _sgm.UpdateSelectionUI();
    }

    private IEnumerator Move(Ships ship, int x, int y)
    {
        _gbm.MoveShip(ship, x, y);
        // TODO make the movement animation
        yield return new WaitForSeconds(1.0f);
        _stage = Stages.Enemies;
        _player.ConsumeEnergyByTurn();
        _sgm.UpdateSelectionUI();
    }

    private IEnumerator SwitchShield(Ships player)
    {
        _sm.IsClicked = false;
        _sm.SwitchShield(player);
        yield return new WaitForSeconds(1.0f);
        _stage = Stages.Enemies;
    }

    public void LoadGame(int index)
    {
        SceneManager.LoadScene(index);
    }

    private void ShowLoseScreen()
    {
        _Igm.ShowLoseScreen();
    }

    private void ShowWinScreen()
    {
        _Igm.ShowWinScreen();
    }

    private void DisableContinueScreen()
    {
        _Igm.DisableContinueScreen();
    }

    public void RestartWholeGame()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartKeepLevel()
    {
        DisableContinueScreen();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartNextLevel()
    {
        GlobalInformation.CurrentLevelIndex =
            Math.Min(++GlobalInformation.CurrentLevelIndex, (_gbm.GetLevelCount() - 1));
        DisableContinueScreen();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartPreviousLevel()
    {
        GlobalInformation.CurrentLevelIndex = Math.Max(--GlobalInformation.CurrentLevelIndex, 0);
        DisableContinueScreen();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void DisableRestartButton()
    {
        _restartButton.SetActive(false);
    }

    private IEnumerator RunWatsonCommand()
    {
        Debug.Log("Running Watson Command.");
        // wait for WatsonManager
        while (_wm.IsWatsonRunning()) yield return null;
        var output = _wm.GetFinalResult();
        _wm.ResetFinalResult();
        var index = _sgm.GetWholeIndexFromSelection(output.SelectionIndex);
        switch (output.Intent)
        {
            case WatsonIntents.Attack:
                if (_gbm.IsEnemy(index))
                {
                    StartCoroutine(AttackEnemy(_player, _gbm.GetShip(index)));
                }
                else
                {
                    var message = $"Nothing to be attacked on {_sgm.GetIndexNameString(output.SelectionIndex)}";
                    ShowMessage(message, Stages.Player);
                    Debug.Log($"Fail to attack {index}");
                }

                break;
            case WatsonIntents.Move:
                if (_gbm.IsEmpty(index))
                {
                    StartCoroutine(Move(_player, index.x, index.y));
                }
                else
                {
                    var message = $"You can't move to {_sgm.GetIndexNameString(output.SelectionIndex)}";
                    ShowMessage(message, Stages.Player);
                    Debug.Log($"Fail to move {index}");
                }

                break;
            default: // fail
                // message box;
                ShowMessage(output.FailMessage, Stages.Player);
                break;
        }
    }

    private void SetPanelPosition()
    {
        if (_player.ShipObject != null)
        {
            var pos = _player.ShipObject.transform.position;
            _questionCanvas.transform.parent.position = new Vector3(pos.x, pos.y + _panelHigh, pos.z);
            _selectionCanvas.transform.parent.position = new Vector3(pos.x, pos.y + _panelHigh, pos.z);
            _messageCanvas.transform.parent.position = new Vector3(pos.x, pos.y + _panelHigh, pos.z);
        }
    }

    private enum PanelType
    {
        Question,
        Selection,
        Message
    };
}