using Microsoft.AspNetCore.Components.Server.Circuits;

namespace DotnetVibe.Web.Services;

public sealed class AccessTokenCircuitHandler(
    AccessTokenProvider accessTokenProvider,
    CircuitAccessTokenStore circuitAccessTokenStore) : CircuitHandler
{
    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitContext.CurrentCircuitId = circuit.Id;
        await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CircuitContext.CurrentCircuitId = circuit.Id;
        await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitAccessTokenStore.RemoveToken(circuit.Id);

        if (CircuitContext.CurrentCircuitId == circuit.Id)
        {
            CircuitContext.CurrentCircuitId = null;
        }

        return Task.CompletedTask;
    }
}
