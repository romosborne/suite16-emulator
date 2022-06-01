public class State
{
    public Room[] Rooms;
    public Input[] Inputs;

    public State(){
        Rooms = new Room[16];
        Inputs = new Input[16];

        for (int i = 0; i < 16; i++)
        {
            Rooms[i] = new Room();
            Inputs[i] = new Input();
        }
    }
}