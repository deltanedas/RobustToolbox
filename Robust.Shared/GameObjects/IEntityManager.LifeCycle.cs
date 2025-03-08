namespace Robust.Shared.GameObjects;

public partial interface IEntityManager
{
    /// <summary>
    /// Force-sets the life stage of an entity's MetaDataComponent to a certain stage.
    /// Do not use this if you don't know what you are doing!
    /// </summary>
    void SetLifeStage(MetaDataComponent meta, EntityLifeStage stage)
    {
        meta.EntityLifeStage = stage;
    }
}
