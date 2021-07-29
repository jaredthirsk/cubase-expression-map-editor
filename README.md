# cubase-expression-map-editor
Cubase Expression Map Editor: a limited editor for automating a few parts of creating expression maps.  See instructions to get an idea of what is automated.

## Instructions

In Cubase:
1. In Cubase: Create groups for your expression map
1. In Cubase: Create articulations for your expression map, and fill in all the groups.  Skip the name. (Leave name as Slot 123, etc.)
1. In Cubase: Save your .expressionmap file to disk
1. In Editor: Open the .expressionmap file.  Heads up: autosave is enabled and will overwrite this file as you make certain changes.
1. In Editor: In the header row, click the Left chevron to assign all recommended names to overwrite Slot 1, Slot 2, etc., or click the left chevron for each row you wish to use an auto-generated name for.  (To tweak auto-generated names, edit the code.)
1. In Editor: Filter the list of articulations using the header row dropdown lists.  Check all articulations you wish to edit, or click the checkbox in the header row to select/deselect all.  Then click a note combo, such as B-2, and click Add Note.  This will add that note to all the selected visible articulations.  Things are auto-saved
1. In Editor: click a note if you wish to remove it.  (See Known issues when deleting all notes.)
1. Close Editor
1. In Cubase: load the .expressionmap and verify all articulations are there, (and the names and notes you added show up).  If some articulations are totally missing, look at the known issues.

That's it!  A lot more could be automated, and more error checks could be added, but this is the bulk of what I needed so I stopped here.

## Known issues

 - If you remove all notes in an articulation and save, Cubase will only load part of the .expressionmap.  DATA LOSS!  The problem is Cubase doesn't load self-closing XML elements.  So there needs to be a search and replace of 
 <list name="obj" type="obj" /> with <list name="obj" type="obj"></list>, and then try loading again in Cubase and all the articulations should load.