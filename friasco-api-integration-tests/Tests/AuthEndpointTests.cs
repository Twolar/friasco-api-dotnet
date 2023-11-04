namespace friasco_api_integration_tests.Tests;

public partial class AuthEndpointTests : IntegrationTestBase
{
    // TODO: Test all user route authentication (do this as passing in test parameters and changing tests to suit)
    // GetAll
    // GetById
    // Create
    // Update
    // Delete

    // Leaving here for now decide where to put them, need to test:
    // Login
    // Register (CreateUser)
    // Tiered based role auth, i.e
    //      If user is superadmin then should have access to everything below this
    //      If user is admin should have admin and below, but not superadmin
}
