using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using SevenSigils.Api.Controllers;

namespace SevenSigils.Api.BackOffice;

/// <summary>
/// Contrôleurs du back-office (« citadel ») : authentification et administration.
/// Quand Admin:Enabled vaut false (production), ils sont retirés de l'application —
/// leurs routes n'existent pas (404), ce qui supprime toute la surface d'attaque
/// liée au login et aux écritures, au lieu de seulement la protéger (401).
/// </summary>
public static class BackOfficeControllers
{
    public static readonly IReadOnlySet<Type> Types = new HashSet<Type>
    {
        typeof(AuthController),
        typeof(AdminController),
        typeof(AdminQuotesController),
    };

    /// <summary>
    /// À appeler après builder.Build() (configuration finale) et avant le premier
    /// routage : la liste des contrôleurs n'est calculée qu'à ce moment-là.
    /// </summary>
    public static void Remove(ApplicationPartManager partManager) =>
        partManager.FeatureProviders.Add(new RemovingFeatureProvider());

    // Ajouté en dernier : s'exécute après le ControllerFeatureProvider par défaut
    // et retire les contrôleurs qu'il a découverts.
    private sealed class RemovingFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in feature.Controllers.Where(IsBackOffice).ToList())
                feature.Controllers.Remove(controller);
        }

        private static bool IsBackOffice(TypeInfo controller) => Types.Contains(controller.AsType());
    }
}
