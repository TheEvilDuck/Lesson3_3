using UnityEngine;

[CreateAssetMenu(fileName = "Spawner weights config", menuName = "Configs/Spawner/New spawner weight config")]
public class SpawnerWeightConfig : ScriptableObject
{
    [SerializeField,Range(0,1000)]int _maxWeight;
    [SerializeField,Range(0,1000)]int _orcWeight;
    [SerializeField,Range(0,1000)]int _humanWeight;
    [SerializeField,Range(0,1000)]int _elfWeight;

    public int MaxWeight=>_maxWeight;
    public int OrcWeight=>_orcWeight;
    public int HumanWeight=>_humanWeight;
    public int ElfWeight=>_elfWeight;
}
