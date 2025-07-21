// file that holds every game event

public class GameEvent { }

public class OnLevelStart : GameEvent { }
public class OnPlayerCaught : GameEvent { }
public class OnEndLevel : GameEvent
{
    public bool haveSucceded;
}