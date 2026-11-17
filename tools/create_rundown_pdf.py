#!/usr/bin/env python3
"""Build the illustrated Yokai Wave PDF from checked-in documentation.

Requires reportlab, Pillow, Graphviz (dot). Output is written to docs/.
"""
from __future__ import annotations

import re
import subprocess
from pathlib import Path
from xml.sax.saxutils import escape

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (BaseDocTemplate, Frame, Image, KeepTogether,
                                NextPageTemplate, PageBreak, PageTemplate,
                                Paragraph, Spacer, Table, TableStyle)

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
OUTPUT = DOCS / "Yokai_Wave_Project_Rundown.pdf"
BRASS = colors.HexColor("#d4ad68")
INK = colors.HexColor("#1a302c")
GREEN = colors.HexColor("#3e7264")
LIGHT = colors.HexColor("#eef3ee")
PALE = colors.HexColor("#f9f5eb")
GREY = colors.HexColor("#455d56")
PAGE_W, PAGE_H = A4


def prepare_fonts():
    regular = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf")
    bold = Path("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf")
    pdfmetrics.registerFont(TTFont("DVSans", str(regular)))
    pdfmetrics.registerFont(TTFont("DVSansBold", str(bold)))


def styles():
    return {
        "cover_kicker": ParagraphStyle("CoverKicker", fontName="DVSansBold", fontSize=10,
                                        leading=16, textColor=BRASS, spaceAfter=12),
        "cover_title": ParagraphStyle("CoverTitle", fontName="DVSansBold", fontSize=31,
                                       leading=38, textColor=INK, spaceAfter=11),
        "cover_sub": ParagraphStyle("CoverSub", fontName="DVSans", fontSize=13,
                                     leading=22, textColor=GREY, spaceAfter=14),
        "h1": ParagraphStyle("H1", fontName="DVSansBold", fontSize=18, leading=24,
                             textColor=INK, spaceAfter=12),
        "h2": ParagraphStyle("H2", fontName="DVSansBold", fontSize=12, leading=18,
                             textColor=GREEN, spaceBefore=13, spaceAfter=8),
        "body": ParagraphStyle("Body", fontName="DVSans", fontSize=9, leading=15,
                               textColor=GREY, spaceAfter=9),
        "small": ParagraphStyle("Small", fontName="DVSans", fontSize=7.9,
                                leading=11.6, textColor=INK),
        "rowleft": ParagraphStyle("RowLeft", fontName="DVSansBold", fontSize=7.8,
                                  leading=11.4, textColor=INK),
        "header": ParagraphStyle("TableHeader", fontName="DVSansBold", fontSize=8.2,
                                 leading=12, textColor=colors.white),
        "center": ParagraphStyle("Center", fontName="DVSans", fontSize=9,
                                 leading=15, alignment=TA_CENTER, textColor=GREY),
    }


def page_chrome(canvas, doc):
    canvas.saveState()
    canvas.setFillColor(INK)
    canvas.rect(0, PAGE_H - 17*mm, PAGE_W, 17*mm, stroke=0, fill=1)
    canvas.setFillColor(BRASS)
    canvas.rect(0, PAGE_H - 17*mm, PAGE_W, 1.6*mm, stroke=0, fill=1)
    canvas.setFont("DVSansBold", 9)
    canvas.setFillColor(colors.white)
    canvas.drawString(18*mm, PAGE_H - 11.4*mm, "YOKAI WAVE   /   WAWA CHRONICLES")
    canvas.setStrokeColor(BRASS)
    canvas.line(18*mm, 18*mm, PAGE_W - 18*mm, 18*mm)
    canvas.setFont("DVSans", 8)
    canvas.setFillColor(GREY)
    canvas.drawString(18*mm, 12.3*mm, "Architecture • Gameplay flow • Complete source index")
    canvas.drawRightString(PAGE_W - 18*mm, 12.3*mm, f"PAGE {doc.page}")
    canvas.restoreState()


def report_table(rows, s, widths=(192, 316)):
    formatted = [[Paragraph(escape(str(a)), s["rowleft"]),
                  Paragraph(escape(str(b)), s["small"])] for a, b in rows]
    data = [[Paragraph("FILE / COMPONENT", s["header"]),
             Paragraph("PURPOSE", s["header"])], *formatted]
    table = Table(data, colWidths=list(widths), repeatRows=1, hAlign="LEFT")
    table.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), INK),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, LIGHT]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 11),
        ("RIGHTPADDING", (0, 0), (-1, -1), 8),
        ("TOPPADDING", (0, 0), (-1, -1), 8),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
        ("LINEBELOW", (0, -1), (-1, -1), 0.6, BRASS),
    ]))
    return table


def markdown_sections():
    """Extract headings/two-column rows from the maintained file rundown."""
    text = (DOCS / "FILE_RUNDOWN.md").read_text(encoding="utf-8")
    sections = []
    current = None
    for line in text.splitlines():
        if line.startswith("## "):
            if current:
                sections.append(current)
            current = (line[3:], [])
        elif current and line.startswith("| ") and not line.startswith("| ---"):
            cells = [x.strip().replace("`", "") for x in line.strip("|").split("|")]
            if len(cells) >= 2 and cells[0].lower() not in {"file", "file(s)", "path"}:
                current[1].append((cells[0], cells[1]))
    if current:
        sections.append(current)
    return sections


def build():
    prepare_fonts()
    s = styles()
    for name in ("game_flow", "code_architecture"):
        subprocess.run(["dot", "-Tpng", str(DOCS / f"{name}.dot"),
                        "-o", str(DOCS / f"{name}.png")], check=True)

    doc = BaseDocTemplate(str(OUTPUT), pagesize=A4,
                          leftMargin=18*mm, rightMargin=18*mm,
                          topMargin=27*mm, bottomMargin=25*mm,
                          title="Yokai Wave — Complete Project Rundown",
                          author="Yokai Wave development team")
    frame = Frame(18*mm, 24*mm, PAGE_W-36*mm, PAGE_H-51*mm,
                  leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)
    doc.addPageTemplates([PageTemplate(id="standard", frames=[frame], onPage=page_chrome)])
    blocks = []
    P = lambda t, style="body": Paragraph(t, s[style])

    # Cover / executive summary.
    blocks.extend([
        Spacer(1, 32*mm),
        P("DEVELOPMENT HANDOFF   •   ORGANIZATION + STABILITY", "cover_kicker"),
        P("YOKAI WAVE<br/>THE WAWA CHRONICLES", "cover_title"),
        P("A friendship-driven Godot RPG about an exorcist family, seven troubled districts, and one town worth saving.", "cover_sub"),
        Spacer(1, 14*mm),
        report_table([
            ("Gameplay", "Whisper introduction, seven sequential battles, seven companions, candy and friendship."),
            ("New polish", "Original opening score, scene fades, new-Yokai bonding moments and Orcanos boss title."),
            ("Organization", "Scripts/ and Scenes/ grouped by purpose; stable UID metadata moved with scripts."),
            ("Persistence", "Supabase slots; old world paths still load after the file migration."),
            ("Validation", "Static path/UID checks pass. Godot .NET build and game walkthrough remain for team QA."),
        ], s),
        Spacer(1, 10*mm),
        P("BRANCH   feature/organized-wawa-polish", "h2"),
        P("This handoff stays separate from main until the team verifies it in Godot 4.7.2 .NET. The original collision/map layout is retained.", "body"),
        PageBreak(),
    ])

    # Flow page.
    blocks.extend([
        P("01  /  Player journey", "h1"),
        P("New Game introduces Whisper and the family's way of protecting Wawa: build bonds, help frightened spirits, and grow stronger together. Districts only clear after a win; running away never grants a companion."),
        Spacer(1, 4*mm),
    ])
    # Fit the figure together with its heading on one A4 content frame.
    flow_img = Image(str(DOCS / "game_flow.png"), width=224, height=554, hAlign="CENTER")
    blocks.extend([flow_img, PageBreak()])

    # Seven story stages.
    blocks.extend([
        P("02  /  Seven stages of Wawa", "h1"),
        P("The story has a fixed progression order. Every encounter feeds back into world exploration, with an optional genuine bonding moment when a first-time win earns a new partner."),
        report_table([
            ("OPENING  /  FAMILY HOME", "Whisper, a first candy, and the exorcist family promise."),
            ("1  /  HOUSE 2", "First disturbance; basic battle tutorial."),
            ("2  /  SARI-SARI STORE", "Tattletell joins after victory."),
            ("3  /  GAS STATION", "Cheeksqueek joins after victory."),
            ("4  /  PUREGOLD", "Draggie joins after victory."),
            ("5  /  MCDONALD'S", "Enma joins after victory."),
            ("6  /  SCHOOL", "Wobblewok joins after victory."),
            ("7  /  HAUNTED HOUSE", "Orcanos final boss title, victory, bond and epilogue."),
        ], s),
        Spacer(1, 8*mm),
        P("HOW TO PLAY", "h2"),
        P("WASD / arrow keys: move. I: Party and feeding. Clipboard: journal and route. Finish the marked battle to advance; keep candy for friendship growth and healing interactions."),
        P("Save state contains completed stages, position, candy, collected pickup IDs, friendship feed counts, and active partner selection. Do not reorder YokaiRoster indices: they are saved by index."),
        PageBreak(),
    ])

    # Architecture.
    blocks.extend([
        P("03  /  How the code fits together", "h1"),
        P("The Godot scenes are presentation roots. C# scripts implement behavior; shared static state tracks progression and friendship, and service classes persist snapshots to Supabase."),
        Spacer(1, 12*mm),
        Image(str(DOCS / "code_architecture.png"), width=489, height=343, hAlign="CENTER"),
        Spacer(1, 9*mm),
        P("INTEGRATION NOTES", "h2"),
        P("GameProgress is the single authority for cleared stages. CandyInventory is the single authority for available Yokai, chosen Yokai, candy and friendship. SaveLoadService serializes a GameSaveData snapshot via SupabaseHttp and SupabaseSession."),
        P("SceneManager performs short full-screen fades around navigation. MusicPlayer changes between town, battle and the original Whisper opening track, using volume fades. UI overlays share YokaiTheme colors and typography."),
        PageBreak(),
    ])

    # Comprehensive source index pulled from the maintained markdown.
    blocks.append(P("04  /  Complete file-by-file index", "h1"))
    blocks.append(P("Every behavior script and scene from the original root is listed by its new location. Godot .cs.uid files are preserved next to the associated C# source. The asset folders remain as supplied."))
    for heading, rows in markdown_sections():
        if not rows:
            continue
        blocks.append(P(escape(heading.replace("`", "")), "h2"))
        blocks.append(report_table(rows, s))

    # QA and handoff.
    blocks.extend([
        PageBreak(),
        P("05  /  Testing before merge", "h1"),
        P("Static references, sidecar paths and the OGG audio file have been verified. The important remaining validation is in the actual .NET-enabled Godot editor, then a real save/load game walkthrough."),
        report_table([
            ("BUILD", "Open project.godot in Godot 4.7.2 .NET; import and build Yokai.csproj. Resolve any script load error before testing."),
            ("START", "Create a profile, enter new-game cutscene, confirm Whisper and candy narrative plus the original opening track."),
            ("MOVE", "Walk in all directions as male and female. Confirm the subtle pose cycle and preserved building collisions."),
            ("BATTLE", "Win, lose and Run separately. Only wins unlock the next stage and companion."),
            ("BOND", "At stages 2-7 choose candy, then on another run choose hug; verify actual candy count and friendship changes."),
            ("FINAL BOSS", "Enter Haunted House, review Orcanos reveal, skip/auto-close and verify the epilogue."),
            ("SAVE", "Confirm three account slots, old save compatibility, active Yokai, world position, pickups and candy persistence."),
            ("UI", "Test pause, party, journal, collection, transitions, settings, close buttons and input after switching scenes."),
        ], s),
        Spacer(1, 12*mm),
        P("PROJECT COMMANDS", "h2"),
        P("<b>python3 tools/verify_project_paths.py</b> — check resources and UIDs.<br/><b>git diff --check</b> — check patch formatting.<br/><b>dotnet build Yokai.csproj</b> — requires .NET 8 and Godot.NET.Sdk 4.7.2."),
        Spacer(1, 10*mm),
        P("Refer to docs/QA_CHECKLIST.md for the detailed click-by-click regression checklist and docs/ARCHITECTURE.md for full implementation boundaries.", "body"),
    ])

    doc.build(blocks)
    print("Created", OUTPUT)


if __name__ == "__main__":
    build()
