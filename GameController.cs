/********************************************************
* GameController.cs
* Austen Mesaros
*
* This class controls the game
*********************************************************/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    private const int MAZE_COLS = 19;                                       // column size of maze
    private const int MAZE_ROWS = 19;                                       // row size of maze

    private Coordinate startPos;                                            // start coordinate position
    private Coordinate finishPos;                                           // finish coordinate position
    private Coordinate curPos;                                              // mouse's current coordinate position
    private Coordinate prevPos;                                             // mouse's previous coordinate position

    public GameObject pressAnyKeyText, pressAnyKeyPanel, debugText;         // ui element game objects
    public GameObject mainCamera, povCamera;                                // camera game objects
    public GameObject cube;                                                 // maze building block

    private Vector3 topDownCameraPosition, tempCameraPosition;              // camera positions
    private Quaternion topDownCameraRotation, tempCameraRotation;           // camera rotations

    public MouseBehavior mouse;                                             // the mouse (start) object
    public CheeseBehavior cheese;                                           // the cheese (finish) object
    private bool bGameBegin = false;                                        // flag to indicate when the maze run can begin

    private char[,] maze = new char[MAZE_ROWS, MAZE_COLS];                  // maze grid
    private bool[,] traveledSpace = new bool[MAZE_ROWS, MAZE_COLS];         // record of spaces traveled in maze


    void Start()
    {
        // generate the maze
        generateNewMaze(maze);

        // set the top-down camera position coordinates
        topDownCameraPosition.x = 5;
        topDownCameraPosition.y = 5.25f;
        topDownCameraPosition.z = -10;

        // set the top-down camera position rotation
        topDownCameraRotation = Quaternion.Euler(0, 0, 0);

        // instantiate the start and finish coordinates
        startPos = new Coordinate(int.MinValue, int.MinValue);
        finishPos = new Coordinate(int.MinValue, int.MinValue);

        // cycle through the maze array to locate the start and finish positions coordinate, instantiate the walls, and set entire traveledSpace array to false
        for (int i=0; i<MAZE_ROWS; i++)
        {
            for (int j=0; j<MAZE_COLS; j++)
            {
                // instantiate the walls
                if (maze[i, j].ToString() == "X")
                {
                    Instantiate(cube, new Vector3(.25f + (.5f * j), -.25f + (10 - (i * .5f)), -.25f), Quaternion.identity);
                }

                // find and save the start row and col
                if (maze[i, j].ToString() == "S")
                {
                    mouse.transform.position = new Vector3(.25f + (.5f * j), -.25f + (10 - (i * .5f)), 0);
                    startPos = new Coordinate(i, j);
                }

                // find and save the finish row and col
                if (maze[i, j].ToString() == "F")
                {
                    cheese.transform.position = new Vector3(.25f + (.5f * j), -.25f + (10 - (i * .5f)), -.15f);
                    finishPos = new Coordinate(i, j);
                }

                // set traveledSpace array to false
                traveledSpace[i, j] = false;
            }
        }

        // save the main camera position and rotation
        tempCameraPosition = mainCamera.transform.position;
        tempCameraRotation = mainCamera.transform.rotation;

        // detach the main camera from the mouse
        mainCamera.transform.parent = null;

        // let the mouse know where the finish position is
        mouse.TellMouseFinishPos(new Coordinate(finishPos));

        // instantiate mouse coordinates to the starting coordinate
        curPos = new Coordinate(startPos);
        prevPos = new Coordinate(startPos);
        
        // call the recursive search method and store the result into solution
        bool solution = Search(curPos, prevPos);

        // if a solution is found, activate the panel that allows users to start the game
        if(solution)
        {
            pressAnyKeyPanel.SetActive(true);
            pressAnyKeyText.SetActive(true);
        }
        else
        {
            pressAnyKeyText.GetComponent<Text>().text = "Error! No solution could be found.";
            pressAnyKeyText.SetActive(true);
        }
    } // end Start

	// update is called once per frame
	void Update () {

        // if user has started the game
        if (bGameBegin)
        {
            // start mouse movement
            mouse.bAllowMovement = true;

            // toggle camera view on mouse button down
            if (Input.GetMouseButtonDown(0))
            {
                mainCamera.SetActive(!mainCamera.activeSelf);
                povCamera.SetActive(!povCamera.activeSelf);
            }

            // lerp camera position
            tempCameraPosition.x = Mathf.Lerp(tempCameraPosition.x, topDownCameraPosition.x, 0.025f);
            tempCameraPosition.y = Mathf.Lerp(tempCameraPosition.y, topDownCameraPosition.y, 0.025f);
            tempCameraPosition.z = Mathf.Lerp(tempCameraPosition.z, topDownCameraPosition.z, 0.025f);

            // update the main camera with the modified camera position
            mainCamera.transform.position = tempCameraPosition;

            // lerp camera rotation
            tempCameraRotation.x = Mathf.Lerp(tempCameraRotation.x, topDownCameraRotation.x, 0.025f);
            tempCameraRotation.y = Mathf.Lerp(tempCameraRotation.y, topDownCameraRotation.y, 0.025f);
            tempCameraRotation.z = Mathf.Lerp(tempCameraRotation.z, topDownCameraRotation.z, 0.025f);

            // update the main camera with the modified camera rotation
            mainCamera.transform.rotation = tempCameraRotation;
        }

        // if the mouse has found the cheese
        if(mouse.bFoundTheCheese)
        {
            // stop the mouse
            mouse.bAllowMovement = false;

            // end game
            bGameBegin = false;

            // activate the press any key panel
            pressAnyKeyPanel.SetActive(true);

            // change and activate the congratulations text
            pressAnyKeyText.GetComponent<Text>().text = "The mouse found the cheese! Hooray!\nClick anywhere to restart.";
            pressAnyKeyText.SetActive(true);

            // restart the scene on mouse button down
            if (Input.GetMouseButtonDown(0))
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }
    } // end Update

    // the recursive method for finding the correct maze path
    bool Search(Coordinate curPos, Coordinate prevPos)
    {
        // if wall or traveled space is encountered, return false
        if (maze[curPos.getRow(), curPos.getCol()] == 'X' || traveledSpace[curPos.getRow(), curPos.getCol()])
        {
            return false;
        }
        else
        {
            mouse.SaveToMousePath(curPos);
        }

        // end found, so return true
        if (curPos == finishPos)
        {
            return true;
        }

        // consider the current space a traveled space
        traveledSpace[curPos.getRow(), curPos.getCol()] = true;

        // check each direction recursively
        if (curPos.getCol() > 1)
        {
            if (Search(new Coordinate(curPos.getRow(), curPos.getCol()-1), curPos))  // Check left
            {
                return true;
            }
        }
        if (curPos.getRow() >= 1)
        {
            if (Search(new Coordinate(curPos.getRow()-1, curPos.getCol()), curPos))  // Check up
            {
                return true;
            }
        }
        if (curPos.getCol() != MAZE_COLS-1)
        {
            if (Search(new Coordinate(curPos.getRow(), curPos.getCol()+1), curPos))  // Check right
            {
                return true;
            }
        }
        if (curPos.getRow() != MAZE_ROWS-1)
        {
            if (Search(new Coordinate(curPos.getRow()+1, curPos.getCol()), curPos))  // Check down
            {
                return true;
            }
        }

        // save the last position to the mouse path
        mouse.SaveToMousePath(prevPos);

        // new direction could not be found, return false
        return false;
    } // end Search

    public void generateNewMaze(char[,] maze)
    {
        // initialize maze as all walls
        for (int t = 0; t < MAZE_ROWS; t++)
        {
            for (int j = 0; j < MAZE_COLS; j++)
            {
                maze[t,j] = 'X';
            }
        }

        // call the recursive maze creation method
        mazeCreate(7, 7);

        // set mouse's starting position as first open space
        for (int i = 1; i < MAZE_ROWS; i++)
        {
            for (int j = 1; j < MAZE_COLS; j++)
            {
                if (maze[i, j] == ' ')
                {
                    maze[i,j] = 'S';
                    break;
                }
            }
            break;
        }

        // set cheese's position as last open space
        for (int i = MAZE_ROWS-2; i > 1; i--)
        {
            for (int j = MAZE_COLS-2; j > 1; j--)
            {
                if (maze[i, j] == ' ')
                {
                    maze[i, j] = 'F';
                    break;
                }
            }
            break;
        }
    }

    public void mazeCreate(int row, int col)
    {
        int i, dir;
        int[,] directions = new int[4, 2] {
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 }
        };

        while (true)
        {
            i = 0;

            // check each direction to see which coordinates are allowed, and save those coordinates into an array
            if (row > 2 && maze[row-2, col] != ' ')  // check two squares up
            {
                directions[i,0] = row - 2;
                directions[i,1] = col;
                i++;
            }
            if (row < MAZE_ROWS - 3 && maze[row+2, col] != ' ')  // check two squares down
            {
                directions[i,0] = row + 2;
                directions[i,1] = col;
                i++;
            }
            if (col > 2 && maze[row, col-2] != ' ')  // check two squares left
            {
                directions[i,0] = row;
                directions[i,1] = col - 2;
                i++;
            }
            if (col < MAZE_COLS - 3 && maze[row, col+2] != ' ')  // check two squares right
            {
                directions[i,0] = row;
                directions[i,1] = col + 2;
                i++;
            }
            
            // if i is never incremented, this means its a dead end
            if (i == 0)
            {
                return;
            }

            // pick a random direction
            dir = Random.Range(0, i);

            // remove a wall
            maze[directions[dir, 0], directions[dir, 1]] = ' ';

            // clear path to removed wall
            maze[(directions[dir, 0] + row) / 2, (directions[dir, 1] + col) / 2] = ' ';

            // recursively call mazeCreate with new position
            mazeCreate(directions[dir,0], directions[dir,1]);
        }
    }

    // hides the ui element and raises the game begin flag
    public void StartTheGame()
    {
        // hide the ui elements
        pressAnyKeyText.SetActive(false);
        pressAnyKeyPanel.SetActive(false);

        // disable the mouse's collider so that the mouse color can no longer be changed when clicking on it
        mouse.GetComponent<CapsuleCollider>().enabled = false;

        // raise game begin flag
        bGameBegin = true;
    } // end StartTheGame
}
