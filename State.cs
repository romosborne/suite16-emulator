public class State
{
    public Room[] Rooms;

    public State()
    {
        Rooms = new Room[16];

        for (int i = 0; i < 16; i++)
        {
            Rooms[i] = new Room();
        }
    }
}