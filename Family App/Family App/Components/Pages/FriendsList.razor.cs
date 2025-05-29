using DBAccessLibrary;

namespace FamilyApp.Components.Pages
{
    public class FriendsList
{
    private string? errorMessage;
    private bool loaded = false;
    private string? message;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            loaded = false;
            try
            {
                await userservice.GetUserFriendsDBAsync(userservice.user.Id);
                loaded = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                loaded = true;
            }
        }
    }

    private void Reload()
    {
        nav.NavigateTo("/profile", forceLoad: true);
    }

    private void GoBack()
    {
        nav.NavigateTo("/profile");
    }
}
}
