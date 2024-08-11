using Sandbox;

public class SceneObjectFlagsFixer : Component
{
    protected override void OnEnabled()
    {
        base.OnEnabled();

        if ( Components.TryGet<ModelRenderer>( out var modelRenderer ) )
        {
            var so = modelRenderer.SceneObject;
            if ( so.IsValid() )
            {
                so.Flags.IsTranslucent = true;
                // adjust more flags if necessary
            }
        }
    }
}
