using Godot;

public partial class SceneTransition : CanvasLayer
{
    private ColorRect _fadeRect;
    private AnimationPlayer _animationPlayer;
    private static SceneTransition _instance;
    
    public static SceneTransition Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SceneTransition();
            }
            return _instance;
        }
    }
    
    public override void _Ready()
    {
        _instance = this;
        
        // ColorRect para fade
        _fadeRect = new ColorRect();
        _fadeRect.Name = "FadeRect";
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _fadeRect.Color = new Color(0, 0, 0, 0);
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_fadeRect);
        
        // AnimationPlayer
        _animationPlayer = new AnimationPlayer();
        _animationPlayer.Name = "AnimationPlayer";
        AddChild(_animationPlayer);
        
        CreateAnimations();
    }
    
    private void CreateAnimations()
    {
        var library = new AnimationLibrary();
        
        // Fade In animation
        var fadeIn = new Animation();
        fadeIn.Length = 0.5f;
        
        int trackIndex = fadeIn.AddTrack(Animation.TrackType.Value);
        fadeIn.TrackSetPath(trackIndex, "FadeRect:color");
        fadeIn.TrackInsertKey(trackIndex, 0.0, new Color(0, 0, 0, 1));
        fadeIn.TrackInsertKey(trackIndex, 0.5, new Color(0, 0, 0, 0));
        
        library.AddAnimation("fade_in", fadeIn);
        
        // Fade Out animation
        var fadeOut = new Animation();
        fadeOut.Length = 0.5f;
        
        trackIndex = fadeOut.AddTrack(Animation.TrackType.Value);
        fadeOut.TrackSetPath(trackIndex, "FadeRect:color");
        fadeOut.TrackInsertKey(trackIndex, 0.0, new Color(0, 0, 0, 0));
        fadeOut.TrackInsertKey(trackIndex, 0.5, new Color(0, 0, 0, 1));
        
        library.AddAnimation("fade_out", fadeOut);
        
        _animationPlayer.AddAnimationLibrary("", library);
    }
    
    public void TransitionToScene(string scenePath)
    {
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Stop;
        _animationPlayer.Play("fade_out");
        _animationPlayer.AnimationFinished += (animName) =>
        {
            if (animName == "fade_out")
            {
                GetTree().ChangeSceneToFile(scenePath);
                _animationPlayer.Play("fade_in");
                _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            }
        };
    }
    
    public void FadeIn()
    {
        _animationPlayer.Play("fade_in");
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
    }
}
