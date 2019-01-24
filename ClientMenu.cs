using Menu;
using System;
using UnityEngine;

public class ClientMenu : Menu.Menu
{
    public ClientMenu(ProcessManager manager, MultiplayerMod mod) : base(manager, (ProcessManager.ProcessID)(-1))
    {
        Page page;
        pages.Add(page = new Page(this, null, "main", 0));
        // create 12 labels to represent an IP address like 216.058.214.014
        float x = -300f;
        for (int num = 0; num < 4; num++)
        {
            x += 525f;
            for (int digit = 0; digit < 3; digit++)
            {
                page.subObjects.Add(labels[num * 3 + digit] = new MenuLabel(this, page, "0", new Vector2(x, 100f), new Vector2(50, 50), false));
                page.subObjects.Add(new BigArrowButton(this, page, "IPDOWN"+(num*3+digit), new Vector2(x, 50f), 2)); // down arrow
                page.subObjects.Add(new BigArrowButton(this, page, "IPINCR"+(num*3+digit), new Vector2(x, 150f), 0)); // up arrow
                x -= 75f;
            }
        }
        page.subObjects.Add(new SimpleButton(this, page, "Start", "START", new Vector2(100f, 200f), new Vector2(100f, 50f)));
        this.mod = mod;
    }
    
    public override void Singal(MenuObject sender, string message)
    {
        if (message.Substring(0, 2) == "IP")
        {
            MenuLabel[] numLabels = new MenuLabel[3];
            int index = int.Parse(message.Substring(6));
            int numStart = index - index % 3;
            int numIndex = index % 3;
            for (int i = 0; i < 3; i++)
            {
                numLabels[i] = labels[i + numStart];
            }
            if (message.Substring(2, 4) == "DOWN")
            {
                numLabels[numIndex].text = (int.Parse(numLabels[numIndex].text)-1).ToString();
                while (numLabels[numIndex].text == "-1") // 200 - 1 = 2 0 0 - 1 = 2 0 (-1) = 2 (-1) 9 = 1 9 9 = 199
                {
                    numLabels[numIndex].text = "9";
                    numIndex++;
                    if (numIndex == 3) break;
                    numLabels[numIndex].text = (int.Parse(numLabels[numIndex].text)-1).ToString();
                }
                if (numIndex != 3) return; // if we didn't get to the end
                numLabels[0].text = "0"; // set it to 0
                numLabels[1].text = "0";
                numLabels[2].text = "0";
            }
            else if (message.Substring(2, 4) == "INCR")
            {
                numLabels[numIndex].text = (int.Parse(numLabels[numIndex].text)+1).ToString();
                while (numLabels[numIndex].text == "10")
                {
                    numLabels[numIndex].text = "0";
                    numIndex++;
                    if (numIndex == 3) break;
                    numLabels[numIndex].text = (int.Parse(numLabels[numIndex].text)+1).ToString();
                }
                if ((numIndex == 3) || (int.Parse(numLabels[2].text) > 2) || (numLabels[2].text == "2" && int.Parse(numLabels[1].text) > 5) || (numLabels[2].text == "2" && numLabels[1].text == "5" && int.Parse(numLabels[0].text) > 5)) // if it's more than 255
                { // set it to 255
                    numLabels[0].text = "5";
                    numLabels[1].text = "5";
                    numLabels[2].text = "2";
                }
            }
        }
        else if (message == "START")
        {
            MenuLabel[] l = labels;
            string ip = l[2].text+l[1].text+l[0].text+"."+l[5].text+l[4].text+l[3].text+"."+l[8].text+l[7].text+l[6].text+"."+l[11].text+l[10].text+l[9].text; // I don't know why they're backwards like this.
            mod.MakeClientThread(ip);
        }
    }
    
    public MultiplayerMod mod;
    MenuLabel[] labels = new MenuLabel[12];
}