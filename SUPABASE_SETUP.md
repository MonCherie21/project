# Supabase connection (one-time setup)

SupabaseConfig.cs now reads connection settings from a separate file that Git
ignores. Your local values will not be replaced by future pulls.

1. In PowerShell, open the Godot project folder (D:\project\project).
2. Copy the sample file, then open it:

   ~~~powershell
   Copy-Item supabase.example.json supabase.local.json
   notepad supabase.local.json
   ~~~

3. Replace the example URL and key with your project's URL and
   **publishable/anon key** from Supabase.
4. Save the file and restart the game.

Example structure (placeholders, not real credentials):

~~~json
{
  "project_url": "https://YOUR-PROJECT.supabase.co",
  "publishable_key": "YOUR_PUBLISHABLE_KEY"
}
~~~

**Never use a Supabase service_role/secret key or a database password in a
client game.** The local file is ignored by Git. Do not force-add it.

For an exported build, the same format also works in the Godot
user://supabase.local.json path. Environment variables YOKAI_SUPABASE_URL and
YOKAI_SUPABASE_KEY are supported if neither local file exists.

If you previously edited SupabaseConfig.cs, copy only the URL and
publishable/anon key into the new local JSON file. Do not restore the old
SupabaseConfig.cs from a stash.
