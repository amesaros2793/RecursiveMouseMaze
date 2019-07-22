/********************************************************
* MouseBehavior.cs
* Austen Mesaros
*
* This class controls the mouse
*********************************************************/
using System.Collections.Generic;
using UnityEngine;

public class MouseBehavior : MonoBehaviour {

    // used for indicating the mouse's movement direction
    private enum DIRECTION { North, South, East, West }

    // mouse texture
    private const int NUM_OF_FURS = 3;
    public Texture mouse1, mouse2, mouse3, mouse4;
    public GameObject Tex1, Tex2;
    private int selectedColor = 0;

    private Transform mouseChild;                                   // mouse child object containing animation component
    private Vector3 tempMousePosition, newMousePos;                 // mouse position 
    private Quaternion tempMouseRotation, newMouseRotation;         // mouse rotation
   
    public List<Coordinate> mousePath = new List<Coordinate>();     // this stores each step of the path to reach the finish
    private int nextCoordinate = 0;                                 // the index for the next step of the path
    public bool bAllowMovement = false;                             // a flag controlling whether or not the mouse is allowed to move
    public bool bFoundTheCheese = false;                            // a flag controlling whether or not the mouse found the cheese
    private Coordinate finishPos;                                   // so the mouse knows where the finish position is
    private bool bFinishedMoving;                                   // to indicate if the mouse has finished a move
    private float mouseMovementDelay = 0f;                          // how long of a pause should the mouse take between movements
    private float timer = 0f;                                       // used for controlling the timing of the mouse movements

    void Start()
    {
        // Set the temp and new mouse positions and rotations to the position and rotation of this GameObject
        tempMousePosition = this.transform.position;
        newMousePos = this.transform.position;
        tempMouseRotation = this.transform.rotation;
        newMouseRotation = this.transform.rotation;

        // acquire child GameObject of mouse for animation purposes
        mouseChild = this.gameObject.transform.GetChild(0);
    } // end Start

    void Update()
    {
        // if movement is allowed
        if (bAllowMovement)
        {
            // start the timer
            timer += Time.deltaTime;

            // if the timer surpasses the mouse movement delay, move the mouse
            if (timer >= mouseMovementDelay)
            {
                // move the mouse to the position of the next coordinate in mousePath
                MoveToSquare(mousePath[nextCoordinate]);
            }

            // increment nextCoordinate by 1
            if (bFinishedMoving)
            {
                nextCoordinate++;
                bFinishedMoving = false;
            }
        }

        // if the finish has been reached
        if (bFoundTheCheese)
        {
            // change the animation
            if (mouseChild.GetComponent<Animation>().isPlaying == false)
            {
                mouseChild.GetComponent<Animation>().CrossFade("Idle2");
            }
        }
    } // end Update

    public void MoveToSquare(Coordinate coordinate)
    {
        // play the walk animation
        mouseChild.GetComponent<Animation>().CrossFade("Walk");

        // to determine which direction the mouse is moving so it can be rotated correctly
        DIRECTION currentDirection = DIRECTION.North;

        // setting the position the mouse should move to
        newMousePos.x = (.5f * coordinate.getCol()) + .25f;
        newMousePos.y = 10 - ((.5f * coordinate.getRow()) + .25f);

        // lerp the position
        tempMousePosition.x = Mathf.Lerp(tempMousePosition.x, newMousePos.x, 0.25f);
        tempMousePosition.y = Mathf.Lerp(tempMousePosition.y, newMousePos.y, 0.25f);

        // if the next coordinate number is less than the length of the mouse path list
        if (nextCoordinate < mousePath.Count-1)
        {
            // determine the direction the mouse is moving and save it into currentDirection
            if (mousePath[nextCoordinate].getRow() > mousePath[nextCoordinate+1].getRow())
            {
                currentDirection = DIRECTION.North;
            }
            else if (mousePath[nextCoordinate].getRow() < mousePath[nextCoordinate+1].getRow())
            {
                currentDirection = DIRECTION.South;
            }
            else if (mousePath[nextCoordinate].getCol() < mousePath[nextCoordinate+1].getCol())
            {
                currentDirection = DIRECTION.East;
            }
            else if (mousePath[nextCoordinate].getCol() > mousePath[nextCoordinate+1].getCol())
            {
                currentDirection = DIRECTION.West;
            }
        }

        // if mouse has arrived at finishPos
        if (mousePath[nextCoordinate].Equals(finishPos))
        {
            // skip the slerping
            tempMouseRotation = newMouseRotation;

            // mouse has found the cheese
            bFoundTheCheese = true;
        }

        // if the cheese has not been found
        if (bFoundTheCheese == false)
        {
            // set the next rotation based on the direction the mouse is moving
            switch (currentDirection)
            {
                case DIRECTION.North:
                    newMouseRotation = Quaternion.Euler(new Vector3(-90, 90, -90));
                    break;
                case DIRECTION.South:
                    newMouseRotation = Quaternion.Euler(new Vector3(90, 90, -90));
                    break;
                case DIRECTION.East:
                    newMouseRotation = Quaternion.Euler(new Vector3(0, 90, -90));
                    break;
                case DIRECTION.West:
                    newMouseRotation = Quaternion.Euler(new Vector3(180, 90, -90));
                    break;
            }
        }

        // slerp the rotation
        tempMouseRotation = Quaternion.Slerp(tempMouseRotation, newMouseRotation, 5f * Time.deltaTime);

        // if the mouse is within the specified distance to the next coordinate
        if (Mathf.Abs(this.transform.position.x - tempMousePosition.x) <= .001f && Mathf.Abs(this.transform.position.y - tempMousePosition.y) <= .001f)
        {
            // reset the timer
            timer = 0.0f;

            // update the tempMousePositionition to the newMousePosition
            tempMousePosition = newMousePos;
            
            // ready to move again
            bFinishedMoving = true;
        }
        
        // update the mouse position and rotation
        this.transform.position = tempMousePosition;
        this.transform.rotation = tempMouseRotation;
    } // end MoveToSquare

    // save a step of the path to the mousePath
    public void SaveToMousePath(Coordinate coordinate)
    {
        mousePath.Add(coordinate);
    } // end SaveToMousePath

    // tell the mouse the finish position
    public void TellMouseFinishPos(Coordinate coordinate)
    {
        finishPos = new Coordinate(coordinate.getRow(), coordinate.getCol());
    } // end TellMouseFinishPos

    // change fur color to next fur color on mouse down
    public void OnMouseDown()
    {
        // cycle selected color
        if (selectedColor < NUM_OF_FURS)
            selectedColor++;
        else
            selectedColor = 0;

        // set the main texture to the selected color
        switch (selectedColor)
        {
            case 0:
                Tex1.GetComponent<Renderer>().material.mainTexture = mouse1;
                Tex2.GetComponent<Renderer>().material.mainTexture = mouse1;
                break;
            case 1:
                Tex1.GetComponent<Renderer>().material.mainTexture = mouse2;
                Tex2.GetComponent<Renderer>().material.mainTexture = mouse2;
                break;
            case 2:
                Tex1.GetComponent<Renderer>().material.mainTexture = mouse3;
                Tex2.GetComponent<Renderer>().material.mainTexture = mouse3;
                break;
            case 3:
                Tex1.GetComponent<Renderer>().material.mainTexture = mouse4;
                Tex2.GetComponent<Renderer>().material.mainTexture = mouse4;
                break;
            default:
                Tex1.GetComponent<Renderer>().material.mainTexture = mouse1;
                Tex2.GetComponent<Renderer>().material.mainTexture = mouse1;
                break;
        }
    } // end OnMouseDown
}