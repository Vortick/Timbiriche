using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GManager : MonoBehaviour
{
    public static GManager instance;
    [SerializeField] int[] boardSize = new int[2];
    public Color Player1Color;
    public Color Player2Color;
    [SerializeField]
    Text txt_player1Score;
    [SerializeField]
    Text txt_player2Score;
    int PScore1 =0;
    int PScore2 =0;

    public Color HighlightColor;
    bool isPLayer1=true;
    bool isPVP=false;

    [SerializeField] Image[] BoxImages;
    [Header("Button Settings, place horizontal buttons first")]
    [SerializeField] Button[] nodeButtons;
    [SerializeField] TimbiricheBox[] Boxes;
    List<int> startedBoxes = new List<int>();

    List<int> freeNodes = new List<int>();
    
    public bool IsPLayer1 { get => isPLayer1;}

    private void Start()
    {
        instance = this;
        PopulateSquaresInfo();
    }

    void PopulateSquaresInfo()
    {
        int[] adyacentNodes = new int[4];
        int row;
        int displacement = boardSize[0] * (boardSize[1] + 1)-1;
        Boxes = new TimbiricheBox[BoxImages.Length];
        Debug.Log("Total Buttons " + nodeButtons.Length + " Displacement " + displacement);


        for (int i = 0; i < BoxImages.Length; i++)
        {
           row = i / boardSize[0];
            Boxes[i] = new TimbiricheBox();
            adyacentNodes[0] = i ;
            adyacentNodes[1] = i + boardSize[0];
            adyacentNodes[2] = i + row + displacement + 1;
            adyacentNodes[3] = i + row + 2 + displacement;
            Debug.Log("Box " + i + ", adyacent nodes " +
                adyacentNodes[0] + "," +
                adyacentNodes[1] + "," +
                adyacentNodes[2] + "," +
                adyacentNodes[3]);
            for (int j = 0; j < 4; j++)
            {                
                Boxes[i].linkedNodes[j] = nodeButtons[adyacentNodes[j]];
                nodeButtons[adyacentNodes[j]].gameObject.name = ("Node " + adyacentNodes[j]);

                //nodeButtons[adyacentNodes[j]].onClick.AddListener(delegate { TakeNode(adyacentNodes[j]); });
                //Debug.Log(nodeButtons[adyacentNodes[j]].name + " should subscribed with number " + adyacentNodes[j]);
            }
                Boxes[i].subcribeButtons();
            Boxes[i].boxImage = BoxImages[i];            
        }
        setAvailableNodes();
        reStart();
    }
    #region Game Logic
    public void TakeNode(int node)
    {
        Debug.Log("Button was clicked " + node);
        if (!nodeButtons[node].interactable)
            return;
        //Debug.Log("Button was clicked " + node);
        nodeButtons[node].interactable = false;
        if (freeNodes.Count>1)
        {
            freeNodes.Remove(node);
            switchPlayer();
        }
        else
        {
            finishGame();
        }
    }
    public void reStart()
    {
        PScore1 = 0;
        PScore2 = 0;
        txt_player2Score.text = ""+0; 
        txt_player1Score.text = ""+0;
        for (int i = 0; i < Boxes.Length; i++)
        {
            Boxes[i].ResetSquare(i);
        }
    }

    public void boxCompleted (Image img, int index) 
    {
        if (IsPLayer1)
        {
            img.color = Player1Color;
            PScore1++;
            txt_player1Score.text = ""+PScore1;
        }
        else
        {
            img.color = Player2Color;
            PScore2++;
            txt_player2Score.text = ""+PScore2; 
        }
        startedBoxes.Remove(index);
        switchPlayer();
    }

    public void switchPlayer()
    {
        if (!isPLayer1 && !isPVP)
        {
            IATakesNode();
        }
        isPLayer1 = !isPLayer1 ;
    }
   

    void finishGame()
    {

    }
    #endregion

    #region IA Setup
    void setAvailableNodes()
    {
        for (int i = 0; i < nodeButtons.Length; i++)
        {
            freeNodes.Add(i);
        }
    }
    public void AddSquareToSolution(int index)
    {
        if (!startedBoxes.Contains(index))
        {
            startedBoxes.Add(index);
        }
    }

    void IATakesNode()
    {
        //tries to complete a box
        if (startedBoxes.Count>0)
        {
            foreach (int item in startedBoxes)
            {
                if (Boxes[item].isReadyToTake())
                {
                    Boxes[item].tryTakingBox();
                    Debug.Log("IA took node " + freeNodes[item]);
                    return;
                }
            }
        }
        if (freeNodes.Count > 2)
        {
            int nodeIndex = Random.Range(0, freeNodes.Count);
            nodeButtons[freeNodes[nodeIndex]].onClick.Invoke();
            Debug.Log("IA took node " + freeNodes[nodeIndex]);
        }
        else
        {
            nodeButtons[freeNodes[0]].onClick.Invoke();
            Debug.Log("IA took node " + freeNodes[0]);
        }
    }
    #endregion
    public void makeImgBlink(Image imag)
    {
        StartCoroutine(blinkBox(imag));
    }
    public IEnumerator blinkBox(Image img)
    {
        img.color = Color.green;
        yield return new WaitForSeconds(1);
        img.color = Color.white;
    }
}
[System.Serializable]
public class TimbiricheBox
{
    int myIndex = 0;
    public Image boxImage;
    public Button[] linkedNodes = new Button[4];
    private bool isTaken = false;
    int takenNodes = 0;

    public bool IsTaken { get => isTaken; }
    public bool isReadyToTake()
    {
        return (takenNodes == 3);
    }
    public void tryTakingBox()
    {
        for (int i = 0; i < 4; i++)
        {
            if (linkedNodes[i].interactable)
            {
                linkedNodes[i].onClick.Invoke();
                return;
            }
        }        
    }
    public void subcribeButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            linkedNodes[i].onClick.AddListener(onNodeTakenForBox);                
        }
    }
    void onNodeTakenForBox ()
    {
        Debug.Log("Button clicked near " + myIndex);
        GManager.instance.makeImgBlink(boxImage);
        if (!isTaken)
        {
            GManager.instance.AddSquareToSolution(myIndex);
            takenNodes++;
            if (takenNodes > 3)
            {
                GManager.instance.boxCompleted(boxImage, myIndex);
                isTaken = true;
            }
        }
    } 
    public void ResetSquare(int index)
    {
        isTaken = false;
        myIndex = index;
        boxImage.color = Color.white;
    }


}
// Brief summary of game logic and tought process

//  How would the machine see the game?
// -> given a node, we could calculate connected nodes with math and graph theory;
//    which sounds cool, and will surely work inside an automated system, but its overkill for a demo
// -> We use a simple event-driven system to decide which node to select, if none would score a point
//    it should select a random node
// -> a smarter system would also prevent the player from scoring too many points, 
//    it would also decide the route to score the most points// 
//   
