using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    // masaya atılan kartların yerleşeceği yeri tutar
    Vector3[] cardPositionsOnTable = new Vector3[3], cardPositionsComputer = new Vector3[4];
    public GameObject[] buttons, playerCards, computerCards, tableCards;
    public GameObject table, endPanel;
    Color redColor = new Color(.7f, .35f, .35f);
    public Sprite[] cardImages;
    int[] deck = new int[52], computerDeck = new int[4], playerDeck = new int[4], leftCardCounts = new int[13];
    int cardIndex = 0, PointOnDeck = 0, numberOfCardsOnTable = 3, topCardNumber = -2, playerScore = 0, computerScore = 0,
        playerCardsWon = 0, computerCardsWon = 0;
    public Text playerText, computerText, endScorePlayer, endScoreComputer, endText;
    
    void Start()
    {
        // bilgisayarın hamlelerini mantıklı yapması için henüz açılmamış olan kartların sayısını tutar
        for(int i = 0; i < 13; i++) leftCardCounts[i] = 4;
        // yeni kartlar dağıtılırken kartların yerleşeceği pozisyonları tutar
        for(int i = 0; i < 4; i++) cardPositionsComputer[i] = computerCards[i].transform.position;
        for(int i = 0; i < 3; i++) cardPositionsOnTable[i] = tableCards[i].transform.position;
        // deste yaratılır ve karılır
        CreateDeck();
        // masaya atılan üç kartın puanları toplanır
        for( ; cardIndex < 3; cardIndex++) PointOnDeck += Point(deck[cardIndex]);
        // oyuncu ve bilgisayara 4'er kart dağıtır
        Deal(true);
        ComputerMove();
    }

    // sıra oyuncuya geçtiğinde butonları açar
    void ButtonsInteractable()
    {
        // desteler boş ise kart dağıtır, kartlar bitmişse oyunu bitirir
        if(!CheckDecks(playerDeck)) if(!Deal())
        { 
            StartCoroutine(Win(false, true));
            return;
        }
        // destede henüz masaya atılmayan kartaların butonlarını erişilebilir kılar
        for(int i = 0; i < 4; i++) if(playerDeck[i] != -1) buttons[i].GetComponent<Button>().interactable = true;
    }

    // Destelerin boş olup olmadığını kontrol eder
    bool CheckDecks(int[] checkDeck)
    {
        foreach(int i in checkDeck) if(i != -1) return true;
        return false;
    }

    void ComputerMove()
    {
        // desteler boş ise kart dağıtır, kartlar bitmişse oyunu bitirir
        if(!CheckDecks(computerDeck)) if(!Deal())
        { 
            StartCoroutine(Win(true, true));
            return;
        }
        // bilgisayar kart seçer
        int cardIndex = ComputerChoose();
        // kapalı olan kartı açar
        ShowCardFace(computerCards[cardIndex], computerDeck[cardIndex]);
        // kartın parentını değiştirir
        computerCards[cardIndex].transform.SetParent(table.transform);
        // masadaki puanı artırır
        PointOnDeck += Point(computerDeck[cardIndex]);
        // kartı ekranda hareket ettirir
        StartCoroutine(Move(computerCards[cardIndex], cardPositionsOnTable[numberOfCardsOnTable % 3]));
        // masadaki kart sayısını artırır
        numberOfCardsOnTable++;
        if(topCardNumber % 13 == computerDeck[cardIndex] % 13 || computerDeck[cardIndex] % 13 == 10)
        { 
            StartCoroutine(Win(false, false));
        }
        else
        { 
            topCardNumber = computerDeck[cardIndex];   // en üstteki kart numarasını kaydeder
            ButtonsInteractable();
        }
        // desteye -1 yazdırarak o kartın artık mevcut olmadığını ifade eder
        computerDeck[cardIndex] = -1;
    }

    IEnumerator Win(bool playerOrNot, bool end)
    {
        if(numberOfCardsOnTable == 2 && !end) PointOnDeck += 10; 
        // score yazdır
        if(playerOrNot)
        {
            playerCardsWon += numberOfCardsOnTable;
            playerScore += PointOnDeck;
            playerText.text = "Oyuncu | " + playerCardsWon.ToString() + " | " + playerScore.ToString();
        }
        else
        {
            computerCardsWon += numberOfCardsOnTable;
            computerScore += PointOnDeck;
            computerText.text = "Bilgisayar | " + computerCardsWon.ToString() + " | " + computerScore.ToString();
        }
        topCardNumber = -2;
        Vector3 tablePosition = table.transform.position;
        yield return new WaitForSeconds(1.1f);
        StartCoroutine(Move(table, tablePosition + (playerOrNot ? -2000f : 2000f) * Vector3.up));
        yield return new WaitForSeconds(1.1f);
        //Transform[] transform_list = table.GetComponentsInChildren<Transform>();
        int childCount = table.transform.childCount;
        for(int i = 0; i < childCount; i++) table.transform.GetChild(0).SetParent(transform);
        table.transform.position = tablePosition;
        numberOfCardsOnTable = 0;
        PointOnDeck = 0;
        if(end) 
        {
            GameEnd();
            yield break;
        }
        if(playerOrNot) ComputerMove();   // sırayı bilgisayara devreder
        else ButtonsInteractable();
    }

    // oyun bitimi
    void GameEnd()
    {
        endPanel.SetActive(true);
        // scoreları yazdırıp kazananı belirler
        if(playerCardsWon > computerCardsWon)
        {
            endScoreComputer.text = computerScore.ToString();
            endScorePlayer.text = playerScore.ToString() + "+3";
            if(playerScore + 3 >= computerScore) endText.text = "You Win";
            else endText.text = "You Lose";
        }
        else if(playerCardsWon < computerCardsWon)
        {
            endScoreComputer.text = computerScore.ToString() + "+3";
            endScorePlayer.text = playerScore.ToString();
            if(playerScore>= computerScore + 3 ) endText.text = "You Win";
            else endText.text = "You Lose";
        }
        else
        {
            endScoreComputer.text = computerScore.ToString();
            endScorePlayer.text = playerScore.ToString();
            if(playerScore >= computerScore) endText.text = "You Win";
            else endText.text = "You Lose";
        }
    }

    // yeni oyun
    public void PlayButton()
    {
        SceneManager.LoadScene(0);
    }

    // oyundaki kart sayılarını takip edip en uygun seçimi yapar
    int ComputerChoose()
    {
        // masada kart yoksa ortaya vale harici bir kart atar
        if(topCardNumber == -2) for(int i = 0; i < 4; i++) if(computerDeck[i] != -1) if(computerDeck[i] % 13 != 10) return i;
        int choosenCardOrder = -1;
        for(int i = 0; i < 4; i++) if(computerDeck[i] != -1)  // değer -1 ise o indexte kart yok demektir
        {
            // masadaki kart ile aynı olan var mı kontrolü yapar
            if(computerDeck[i] % 13 == topCardNumber % 13) return i;
            // elde vale var mı kontrolü yapar
            if(computerDeck[i] % 13 == 10) choosenCardOrder = i;
        }
        // eğer masadaki ile aynı kart yok ama vale var ise valseyi atar
        if(choosenCardOrder > -1) return choosenCardOrder;
        int leastCardCount = 5;
        for(int i = 0; i < 4; i++) if(computerDeck[i] != -1)
            // daha önce en fazla açılan kartı seçer
            if(leftCardCounts[computerDeck[i] % 13] < leastCardCount) 
            {
                choosenCardOrder = i;
                leastCardCount = leftCardCounts[computerDeck[i] % 13];
            }
        return choosenCardOrder;
    }

    // select card button function 
    public void ChooseCard(int cardIndex)
    {
        // seçilen kartın parentını table yapar
        playerCards[cardIndex].transform.SetParent(table.transform);
        // masadaki puanı artırır
        PointOnDeck += Point(playerDeck[cardIndex]);
        // seçilen kartı hareket ettirir
        StartCoroutine(Move(playerCards[cardIndex], cardPositionsOnTable[numberOfCardsOnTable % 3]));
        // masadaki kart sayısını artırır
        numberOfCardsOnTable++;
        // butonları kapatır
        foreach(GameObject button in buttons) button.GetComponent<Button>().interactable = false;
        if(topCardNumber % 13 == playerDeck[cardIndex] % 13 || playerDeck[cardIndex] % 13 == 10) StartCoroutine(Win(true, false));
        else
        { 
            // en üsteki kart bilgilerini günceller
            topCardNumber = playerDeck[cardIndex];
            // sırayı bilgisayara devreder
            Invoke("ComputerMove", 1.2f);  
        }
        // atılan kartın destedeki yerinin boş olduğunu ifade eder
        playerDeck[cardIndex] = -1;      
    }

    void CheckDecks(){}

    // oyuncu ve bilgisayara 4'er kart dağıtır
    bool Deal(bool inStart = false)
    {
        if(cardIndex > 43) return false;
        // Oyun başlangıcında kartlar dağıtılırken çağırma
        // En üstteki üç kartı boşta olan kartlarla değiştirir
        if(!inStart)
        {
            int childCount = table.transform.childCount;
            int changingCardCount = childCount > 3 ? 3 : childCount;
            for(int i = 0; i < changingCardCount; i++)
            {
                tableCards[i].transform.position = table.transform.GetChild(childCount - i - 1).position;
                tableCards[i].transform.rotation = table.transform.GetChild(childCount - i - 1).rotation;
                ShowCardFace(tableCards[i], Int32.Parse(table.transform.GetChild(childCount - i - 1).name));
                table.transform.GetChild(childCount - i - 1).SetParent(transform);
                //tableCards[i].transform.SetParent(table.transform);
            }
            for(int i = changingCardCount; i > 0; i--) tableCards[i - 1].transform.SetParent(table.transform);
        }

        for(int i = 0; i < 4; i++)
        {
            // oyuncuya verilen kartların numarasını oyuncunun destesine kaydeder
            playerDeck[i] = deck[cardIndex];
            // oyuncunun kartlarını ekranda doğru yere yerleştirir
            playerCards[i].transform.position = buttons[i].transform.position;
            // kart eğer masadaysa parentını sıfırlar
            playerCards[i].transform.SetParent(transform);
            // oyuncunun kartlarını görünür kılar
            ShowCardFace(playerCards[i], playerDeck[i]);
            // desteden çekilecek kartın sırasını bir artırır
            cardIndex++;
            // yukarıdaki işlemler bilgisayara kart vermek için de tekrarlanır
            computerDeck[i] = deck[cardIndex];
            computerCards[i].transform.position = cardPositionsComputer[i];
            // kart eğer masadaysa parentını sıfırlar
            computerCards[i].transform.SetParent(transform);
            ShowCardBack(computerCards[i]);
            cardIndex++;
        }
        return true;
    }

    // kartların arkasını gösterir
    void ShowCardBack(GameObject card)
    {
        // karttaki yazıyı sıfırlar
        card.transform.GetChild(0).gameObject.GetComponent<Text>().text = "";
        // kartın arkasına ait görüntüyü ekler
        card.GetComponent<Image>().sprite = cardImages[16];
    }

    // kartları görünür kılar
    void ShowCardFace(GameObject card, int cardNumber)
    {
        card.name = cardNumber.ToString();
        // kartın üzerindeki text, karta child olarak konuldu ve aşağıda çağırılıyor
        Text cardText = card.transform.GetChild(0).gameObject.GetComponent<Text>();
        // kartın yazısı yazılır
        if(cardNumber % 13 == 0) cardText.text = "A";
        else if(cardNumber % 13 < 10) cardText.text = (cardNumber % 13 + 1).ToString();
        else cardText.text = "";
        // kartın rengi belirlenir
        if((cardNumber / 13) % 2 == 0) cardText.color = redColor;
        else cardText.color = Color.black;
        // kartın resmi belirlenir
        int order = cardNumber / 13 * 4;
        if(cardNumber % 13 > 9) order += cardNumber % 13 - 9;
        card.GetComponent<Image>().sprite = cardImages[order];
    }

    // gönderilen objeyi hareket ettirir
    IEnumerator Move(GameObject moveObject, Vector3 endPosition)
    {
        // gidilecek yönü hesaplar
        Vector3 directionVector = (endPosition - moveObject.transform.position) / 30f;
        // objeyi hareket ettirir
        for(int i = 0; i < 30; i++)
        { 
            moveObject.transform.Translate(directionVector, Space.World);
            yield return 0;
        }
    }

    void CreateDeck() 
    {     
        // desteye numaralar yerleştirilir   
        for(int  i = 0; i < 52; i++) deck[i] = i;
        int tempNumber, randomOrder;
        // yerleştirilen numaralar karıştırılıp karma deste elde edilir
        for (int i = 0; i < 52; i++) 
        {
            randomOrder = UnityEngine.Random.Range(0, 52);
            tempNumber = deck[randomOrder];
            deck[randomOrder] = deck[i];
            deck[i] = tempNumber;
        }
    }

    //kartların puan değeri
    int Point(int cardNumber)
    {
        if(cardNumber % 13 == 0 || cardNumber % 13 == 10) return 1;   // as ve vale 1 puan
        else if(cardNumber == 40) return 2;   // sinek ikili 2 puan
        else if(cardNumber == 35) return 3;   // karo onlu 3 puan
        else return 0;
    }
}
