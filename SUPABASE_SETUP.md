# Supabase connection for the team

The game can now read a shared JSON file committed to Git. When the project
owner fills in `supabase.shared.json` and commits it, teammates only need to
pull the latest branch. They do not need separate Supabase dashboard accounts.

## One-time setup for the project owner

1. Open `supabase.shared.json` in the Godot project folder.
2. Replace both placeholders with the **Project URL** and the **publishable
   API key** (`sb_publishable_...`) from Supabase Project Settings > API Keys.
   An older **anon** key also works.
3. Commit only this JSON file and the accompanying code changes to Git.
4. Ask a teammate to pull the branch and try New Game, Continue, and saving.

Example structure (placeholders, not real credentials):

~~~json
{
  "project_url": "https://YOUR-PROJECT.supabase.co",
  "publishable_key": "YOUR_PUBLISHABLE_KEY"
}
~~~

**Never put a secret key, service_role key, or database password in this file.**
It is tracked by Git and becomes visible to everyone with repository access.
Only the URL and public client key belong in this file. Ensure Row Level
Security (RLS) protects the `players` and `player_saves` tables.

## Optional personal override

If a developer needs a different Supabase project, they can use an ignored
`supabase.local.json` file. This takes priority over the shared team file.
In PowerShell, open the project folder, then run:

~~~powershell
Copy-Item supabase.example.json supabase.local.json
notepad supabase.local.json
~~~

Enter your personal public connection values in that file. The file is ignored
by Git and will not be replaced during a pull. Another option for an exported
build is `user://supabase.local.json`. Environment variables `YOKAI_SUPABASE_URL`
and `YOKAI_SUPABASE_KEY` are a fallback if neither JSON file contains a valid
public configuration.

Loading order: `res://supabase.local.json`, then
`user://supabase.local.json`, then `res://supabase.shared.json`, then
environment variables. Invalid placeholders are skipped.

## If New Game still cannot connect

1. Confirm that `supabase.shared.json` has real values, not `YOUR-PROJECT`.
2. Ensure the key is a publishable (`sb_publishable_...`) or legacy anon key.
3. Open the game with the Godot .NET editor and check its Output panel.
4. In the Supabase dashboard, confirm anonymous sign-in is enabled under
   Authentication, and that the project's API is reachable.
5. Confirm the `players` and `player_saves` tables exist and their RLS policies
   permit each signed-in player to access their own data.
