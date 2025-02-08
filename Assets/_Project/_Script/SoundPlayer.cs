using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource audioSourceWalk;
    [SerializeField] AudioSource audioSourceJump;

    [Header("Walk Sounds")]
    [SerializeField] AudioClip walkSoundConcrete;
    [SerializeField] AudioClip walkSoundWater;
    [SerializeField] AudioClip walkSoundGrass;

    [Header("Run Sounds")]
    [SerializeField] AudioClip runSoundConcrete;
    [SerializeField] AudioClip runSoundGrass;
    [SerializeField] AudioClip runSoundWater;

    [Header("Other")]
    [SerializeField] AudioClip jumpSound;

    public SurfaceType surfaceType;

    public void PlayJumpSound()
    {
        audioSourceJump.clip = jumpSound;
        audioSourceJump.Play();
    }

    public void ChangeState(MoveState state)
    {
        switch (state)
        {
            case MoveState.None:
                PlayNone();
                break;
            case MoveState.Walk:
                PlayWalkSound();
                break;
            case MoveState.Run:
                PlayRunSound();
                break;
        }
    }

    void PlayWalkSound()
    {
        switch(surfaceType)
        {
            case SurfaceType.Concrete:
                Play(walkSoundConcrete);
                break;
            case SurfaceType.Grass:
                Play(walkSoundGrass);
                break;
            case SurfaceType.Water:
                Play(walkSoundWater);
                break;
        }
    }

    void PlayRunSound()
    {
        switch (surfaceType)
        {
            case SurfaceType.Concrete:
                Play(runSoundConcrete);
                break;
            case SurfaceType.Grass:
                Play(runSoundGrass);
                break;
            case SurfaceType.Water:
                Play(runSoundWater);
                break;
        }
    }


    public void PlayNone()
    {
        Play(null);
    }

    void Play(AudioClip clip)
    {
        if (audioSourceWalk.clip == clip) return;

        audioSourceWalk.clip = clip;
        if (clip != null) audioSourceWalk.Play();
        else audioSourceWalk.Stop();
    }
}

public enum MoveState
{
    None,
    Walk,
    Run
}

public enum SurfaceType
{
    Concrete,
    Grass,
    Water
}
