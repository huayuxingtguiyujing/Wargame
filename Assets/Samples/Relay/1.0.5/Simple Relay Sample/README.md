# Relay SDK Unity Test Project

Unity project to exercise the Relay SDK. The project is intended to be used for manual testing of the SDK. It provides a simple UI with buttons.

## Using the project

Open the project with the Unity editor (2020.3), enter game mode and then click on the buttons. The text fields will be updated with the data returned by the SDK. Keep an eye in the console for errors.

This project depends on the Relay SDK located in this repository. To test a published version, replace the file path in `Pacakges/manifest.json` with a version tag.

By default this project points to a dev (aka test) deployment of the Relay Allocations API in GCP (https://relay-allocations-test.services.api.unity.com). Be sure to click on `Sign In` before `Allocate`/`JoinCode`/`Join`, this will get a UAS token which is required by the Game Backend Gateway (UGG).

The base path of the Relay Allocations API can be changed by setting `RelayService.Configuration.BasePath` (see [RelayAllocations.cs](./Assets/Scripts/RelayAllocation.cs)).

The [project settings](./ProjectSettings/ProjectSettings.asset) contain the `relay-stg` project and organization identifiers. They also have `AUTHENTICATION_TESTING_STAGING_UAS` set as a `scriptingDefineSymbols`, this is needed in order to authenticate against the staging UAS.

### Gotchas
- Make sure you are not signed on in the editor.
- If you only see a blue background, make sure that you have selected the proper scene before pressing play.
