using SpaceMath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Sub managers
    [SerializeField]
    private AttackManager am;
    [SerializeField]
    private GameBoardManager gbm;
    [SerializeField]
    private UIManager uim;
    [SerializeField]
    private SelectionGridManager sgm;

    private Ships player;
    private List<Ships> enemies;

    // Store the game stage.
    public enum Stages { None, Question, Player, Enemies };
    private Stages stage;

    // Drag the canvas into these variables in the inspector
    [SerializeField]
    private GameObject questionCanvas;
    [SerializeField]
    private GameObject selectionCanvas;

    // Game settings
    [SerializeField]
    private int energyConsumption;


    void Start()
    {
        // Question stage
        stage = Stages.Question;
        SwitchPanel(true);
        player = gbm.GetPlayer();
    }

    void Update()
    {
        if (player.IsShipDead())
        {
            // Game over
        }
        else if (!gbm.IsEnemiesRemain())
        {
            // You win
        }
        else
        {
            switch (stage)
            {
                // Question stage
                case Stages.Question:
                    SwitchPanel(true);
                    switch (uim.GetAnswerState())
                    {
                        // Answer was correct
                        case UIManager.AnswerStates.Right:
                            stage = Stages.None;
                            uim.SetAnswerState(UIManager.AnswerStates.Suspension);
                            StartCoroutine(QuestionToPlayerTurn());
                            break;

                        // Answer was incorrect
                        case UIManager.AnswerStates.Wrong:
                            stage = Stages.None;
                            uim.SetAnswerState(UIManager.AnswerStates.Suspension);
                            StartCoroutine(QuestionToEnemiesTurn());
                            break;

                        default:
                            break;
                    }
                    break;

                // Player's turn
                case Stages.Player:
                    SelectionOutput selectionResult = sgm.GetFinalResult();
                    if (selectionResult != null && selectionResult.Type != ActionType.None)
                    {
                        stage = Stages.None;
                        switch (selectionResult.Type)
                        {
                            case ActionType.Move:
                                sgm.ResetFinalResult();
                                StartCoroutine(Move(player, selectionResult.TargetIndex.x, selectionResult.TargetIndex.y));
                                break;
                            case ActionType.Attack:
                                sgm.ResetFinalResult();
                                StartCoroutine(AttackEnemy(player, gbm.GetShip(selectionResult.TargetIndex)));
                                break;
                            //case ActionType.Shield:
                            //   sgm.ResetFinalResult();
                            //   Do something
                            //   break;
                            default:
                                break;
                        }
                    }
                    break;

                // Enemies' turn
                case Stages.Enemies:
                    stage = Stages.None;
                    enemies = gbm.GetEnemiesInRange();
                    StartCoroutine(AttackPlayer(enemies, player));
                    break;

                default:
                    break;
            }
        }
    }

    // Set "on" to true to turn on the question panel.
    private void SwitchPanel(bool on)
    {
        questionCanvas.SetActive(on);
        selectionCanvas.SetActive(!on);
    }

    private IEnumerator QuestionToPlayerTurn()
    {
        yield return new WaitForSeconds(0.8f);
        SwitchPanel(false);
        stage = Stages.Player;
    }

    private IEnumerator QuestionToEnemiesTurn()
    {
        yield return new WaitForSeconds(0.8f);
        questionCanvas.SetActive(false);
        selectionCanvas.SetActive(false);
        stage = Stages.Enemies;
    }

    private IEnumerator AttackEnemy(Ships player, Ships enemy)
    {
        am.Attack(player, enemy);
        yield return new WaitForSeconds(1.0f);
        stage = Stages.Enemies;
        sgm.UpdateSelectionUI();
    }

    private IEnumerator AttackPlayer(List<Ships> enemies, Ships player)
    {
        player.DecreaseEnergy(energyConsumption);
        if (enemies != null)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                am.Attack(enemies[i], player);
                yield return new WaitForSeconds(1.0f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }
        stage = Stages.Question;
    }

    private IEnumerator Move(Ships ship, int x, int y)
    {
        gbm.MoveShip(player, x, y);
        // TODO make the movement animation
        yield return new WaitForSeconds(1.0f);
        stage = Stages.Enemies;
        sgm.UpdateSelectionUI();
    }
}
