using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControllerGeneric : MonoBehaviour
{
    //number of buttons is the amount of buttons required to open the door
    public int numOfButtons = 0;
    //marks if the door will be left open after opening once or clsoe again after the player(s) walk off the button
    public bool permanent = false;
    //the up to four flags on which buttons are stepped on
    bool[] flag = new bool[] { false, false, false, false };
    //the actual door itself
    [SerializeField] GameObject Door;

    //buttonPressed is a boolean for if it's been pressed or not with true meaning the player is on it
    //currentBtton is the numerical idnetifier for the button
    public void doorFunc(bool buttonPressed, int currentButton)
    {
        switch (numOfButtons)
        {
            //1 button
            case 1:
                //if the button is pressed the door will be set as inactive
                if(buttonPressed == true)
                {
                    Door.SetActive(false);
                    //the number of buttons will be set to 0 if permanenet to stop the program
                    if(permanent == true)
                    {
                        numOfButtons = 0;
                    }
                }
                //otherwise the player walked off the button and thus the door will be closed again
                else
                {
                    Door.SetActive(true);
                }
                break;
            //2 buttons
            case 2:
                //if the button is pressed
                if(buttonPressed == true)
                {
                    //the flag for the current button will be set as true
                    flag[currentButton] = true;
                    //it will then check if all the required flags (0 & 1 or 2 & 3) are true
                    //if they are it will open the door and set number of buttons to 0 if it's set as permanenet
                    if ((flag[0] == true && flag[1] == true) || (flag[2] == true && flag[3] == true)) 
                    {
                        Door.SetActive(false);
                        if (permanent == true)
                        {
                            numOfButtons = 0;
                        }
                    }
                }
                //otherwise set the current flag to false and set the door as closed
                else
                {
                    flag[currentButton] = false;
                    Door.SetActive(true);
                }
                break;
            //4 buttons
            case 4:
                //if the button is pressed
                if (buttonPressed == true)
                {
                    //set the current buttons flag as true and check if all four flags are true after
                    flag[currentButton] = true;
                    if (flag[0] == true && flag[1] == true && flag[2] == true && flag[3] == true)
                    {
                        //if they are open the door and set the number of buttons at 0 if it is set to permanenet
                        Door.SetActive(false);
                        if (permanent == true)
                        {
                            numOfButtons = 0;
                        }
                    }
                }
                //otherwise set the current flag to false and close the door
                else
                {
                    flag[currentButton] = false;
                    Door.SetActive(true);
                }
                break;
            default:
                break;
        }
    }

}
