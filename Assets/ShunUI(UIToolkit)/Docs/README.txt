Shun UI (UI Toolkit Version) - Shadcn-Inspired Unity Components

This is not a component library. It is how you build your component library.

For comprehensive documentation, tutorials, and examples, visit:
https://www.experir.com/products/shunui-ui-toolkit/docs

REQUIREMENTS

Shun UI has minimal requirements to keep your project lightweight and focused:
• Unity 6.0 or later - Required for the latest UI Toolkit features

INSTALLATION

Step 1: Test Your Installation
1. Open the Components Scene - Navigate to the included Components scene in your project
2. Run the Scene - Press Play to see all Shun UI components in action
3. Test Interactions - Click buttons, toggle switches, and interact with various components
4. Experiment with Styles - Modify USS variables to see how components update

Step 2: Import Your First Component
1. Create or Open a Scene - Start with a new scene or open an existing one
2. Ensure You Have a UI Document - If you don't have one already, create a UI Document (GameObject → UI Toolkit → UI Document)
3. Import Your First Component - Add Shun UI UXML templates to your UI Document
4. Apply Styles - Reference the component's USS stylesheet or the global USS file
5. Customize Your Components - Once imported, modify the USS styles, UXML structure, and C# scripts to match your needs

Important: UI Toolkit Workflow
UI Toolkit uses UXML for structure, USS for styling, and C# for behavior - offering a web-like development experience within Unity.

PHILOSOPHY

Shun UI solves the problem of traditional Unity UI asset packages where you import assets, drag prefabs, and end up with limited customization options. Inspired by shadcn/ui, it brings a revolutionary philosophy to Unity development.

CORE PRINCIPLES

- Modifiable Components: Once imported, every component is yours to modify and extend
- Composition: Every component uses a common, composable interface built on Unity's UI Toolkit system
- Distribution: Well-organized folder structure with UXML templates and USS stylesheets
- Beautiful Defaults: Carefully chosen default styles and themes using USS

OVERVIEW

Shun UI is a foundation for building modern UI components in Unity. Rather than providing rigid prefabs, it gives you building blocks and patterns to create your own component library.

Inspired by Shadcn/ui
Icons by Lucide (use exicon to add more: https://u3d.as/3NhC)

AVAILABLE COMPONENTS

40+ Component Templates including:

Layout & Navigation:
- Accordion, Breadcrumb, Card, Tabs, MenuBar, NavigationMenu, Separator

Input Components:
- Button, Input, InputOTP, Textarea, Checkbox, RadioGroup, Select, Combobox, 
  Slider, Switch, Toggle, ToggleGroup

Feedback & Display:
- Alert, Badge, Avatar, Icon, Label, Progress, Tooltip

Overlay Components:
- Dialog, AlertDialog, Drawer, Sheet, Popover, HoverCard, HoverMenu, 
  ContextMenu, DropdownMenu

Advanced Components:
- Carousel, Collapsible, ScrollArea, Scrollbar, Resizable, Sonner, DataTable, Skeleton

THEMING SYSTEM

Shun UI includes a complete theming system with runtime theme switching via ShunThemeManager. Themes are ScriptableObject assets that generate USS stylesheets applied to your UIDocument.

Key Features:
- Runtime theme switching without reloading scenes
- Multiple built-in themes (Default, Dark, Square, Red, Rose, Yellow, Green, Blue, Orange, Violet)
- Global USS variables control colors, spacing, borders, and more
- Change a few parameters to update every component
- Consistent design system across all components
- Create custom themes via Create > Shun UI > Theme

BLOCKS

Pre-built composite UI layouts:
- SimpleMainMenu - Main menu game layout with settings panel

NEXT STEPS

Now that you have Shun UI installed and tested:
• Explore Components - Browse through all 40+ available component templates
• Learn the Philosophy - Understand why customizing UXML and USS is crucial
• Experiment with Themes - Modify CSS variables in the global USS to create your own look
• Build Your UI - Start creating your own customized user interface

Remember: This is not a component library. It is how you build your component library. Once imported, every component is yours to modify and extend without restrictions.

BUILDING YOUR COMPONENT LIBRARY

Step 1: Add Components via UI Builder or UXML
- Create a UI Document if needed
- Open UI Builder (Window > UI Toolkit > UI Builder)
- Import Shun UI UXML templates as components
- Unpack the templates to gain full control over their structure, attributes, and USS styles
- Components will be added with their associated USS styles

Step 2: Customize and Make It Yours
- Examine component structure in UI Builder or UXML files
- Modify colors, fonts, spacing using USS stylesheets
- Adjust behavior through C# scripts and VisualElement manipulators
- Create variants by duplicating UXML/USS and modifying
- Build composite components

Step 3: Create Your Own Templates
- Save customized components as your own UXML templates
- Organize USS stylesheets in your project structure
- Build your own component library with reusable templates

CUSTOM THEMES

Step 1: Create a Theme Asset
- Right-click in the Project window
- Select Create > Shun UI > Theme
- Or duplicate an existing theme from the Themes folder

Step 2: Customize Properties
Configure colors (primary, secondary, background, foreground, accent, muted, destructive, etc.), border radius, border width, and other styling properties in the Inspector.

Step 3: Apply Theme
Assign your custom theme to the ShunThemeManager to apply it across all components:
ShunThemeManager.Instance.activeTheme = yourCustomTheme;
ShunThemeManager.Instance.ChangeTheme(customTheme);

DOCUMENTATION

Visit https://www.experir.com/products/shunui-ui-toolkit/docs for:
- Detailed component guides
- Styling customization tutorials
- Integration examples
- Best practices
- API reference
- Roadmap

FILE STRUCTURE

ShunUI/
├── Blocks/        # Pre-built composite UI layouts (UXML)
├── Components/    # Individual UI component templates (UXML)
├── Core/          # Core system files and C# scripts
├── Editor/        # Editor scripts and tools
├── Resources/     # Icons (PNG + SVG) and stylesheets
├── Scenes/        # Example scenes
├── Settings/      # Editor settings
├── Themes/        # Theme configurations
└── Docs/          # Documentation files

ICONS

Icon assets used by ShunUI components are located in the Resources/Icons folder. Only icons
referenced by components are kept here — both PNG and SVG formats are included.

To add new icons, use exicon (https://u3d.as/3NhC) — a Unity editor tool to search and download
icons from open-source libraries (Lucide, Tabler, Heroicons, etc.) directly into your project.

Features:
- Downloads in both PNG and SVG formats at configurable sizes
- Automatically downloads and stores the license file for each icon library
- One-click override to white — the recommended color for Unity UI sprites (tinted at runtime)

CHANGELOG

1.0.3 - February 2026
New Component: Skeleton - Added a skeleton loading placeholder with CSS-like pulse animation using scheduled VisualElement updates.
Icons: Removed unused icons, keeping only those referenced by components. Use exicon (https://u3d.as/3NhC) to download new icons on-demand — supports PNG/SVG, configurable sizes, automatic license downloads, and one-click override to white.
Structure: Moved Settings and Themes folders out of Resources to avoid unnecessary build inclusion. Fonts and Icons remain in Resources (required for runtime loading by UI Toolkit).
Docs: Updated online documentation with Skeleton page, corrected typography and theming pages, and updated Icons page to reflect trimmed icon set and exicon.

1.0.2 - January 2026
New Component: DataTable - Added a comprehensive data table component.
Docs: Updated online documentation with detailed information for every component.

1.0.1 - December 2025
SVG Icon Support (Unity 6.3+): Added VectorImage support for SVG icons across all components. Use iconSvg properties for crisp vector icons that scale perfectly at any size.
Dual Icon System: Components now support both Texture2D (PNG) and VectorImage (SVG) icons. Setting one automatically clears the other for seamless compatibility.
Lucide Icon Library: Included Lucide icons in SVG format in Resources/Icons folder.
Backward Compatibility: PNG icons remain available for Unity 6.2 and earlier versions.
Docs: Added comprehensive Icons documentation page explaining the dual icon system and Unity version compatibility.

1.0.0 - October 2025
First release.

Shun UI - Shadcn-Inspired Unity Components
Version: UI Toolkit Edition - 1.0.3