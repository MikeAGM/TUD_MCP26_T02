using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using Autodesk.Navisworks.Api.Plugins;

namespace GroupClashesByDistance
{
    public class GroupClashesByDistancePlugin : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            Document doc = Autodesk.Navisworks.Api.Application.MainDocument;
            if (doc == null)
            {
                MessageBox.Show(
                    Autodesk.Navisworks.Api.Application.Gui.MainWindow,
                    "Please open a Navisworks file with clash tests first.",
                    "Group Clashes by Distance");
                return 0;
            }

            DocumentClash documentClash = doc.GetClash();
            if (documentClash == null)
            {
                MessageBox.Show(
                    Autodesk.Navisworks.Api.Application.Gui.MainWindow,
                    "Clash Detective is not available in this Navisworks edition.",
                    "Group Clashes by Distance");
                return 0;
            }

            DocumentClashTests testsData = documentClash.TestsData;
            if (testsData == null || testsData.Value?.TestsRoot?.Children == null || testsData.Value.TestsRoot.Children.Count == 0)
            {
                MessageBox.Show(
                    Autodesk.Navisworks.Api.Application.Gui.MainWindow,
                    "No clash tests found. Please create and run at least one clash test.",
                    "Group Clashes by Distance");
                return 0;
            }

            List<ClashTest> clashTests = new List<ClashTest>();
            foreach (SavedItem item in testsData.Value.TestsRoot.Children)
            {
                ClashTest test = item as ClashTest;
                if (test != null)
                    clashTests.Add(test);
            }

            if (clashTests.Count == 0)
            {
                MessageBox.Show(
                    Autodesk.Navisworks.Api.Application.Gui.MainWindow,
                    "No clash tests with results were found.",
                    "Group Clashes by Distance");
                return 0;
            }

            using (GroupClashesByDistanceForm form = new GroupClashesByDistanceForm(clashTests))
            {
                IWin32Window owner = Autodesk.Navisworks.Api.Application.Gui.MainWindow;
                DialogResult dlgResult = form.ShowDialog(owner);

                if (dlgResult != DialogResult.OK)
                    return 0;

                ClashTest selectedTest = form.SelectedTest;
                double threshold = form.DistanceThreshold;

                if (selectedTest == null)
                    return 0;

                List<ClashResult> results = new List<ClashResult>();
                foreach (SavedItem child in selectedTest.Children)
                {
                    ClashResult r = child as ClashResult;
                    if (r != null)
                        results.Add(r);
                }

                if (results.Count == 0)
                {
                    MessageBox.Show(
                        owner,
                        "The selected clash test has no individual clash results (or only groups).",
                        "Group Clashes by Distance");
                    return 0;
                }

                List<List<ClashResult>> groups = GroupResultsByDistance(results, threshold);

                if (groups.Count == 0)
                {
                    MessageBox.Show(
                        owner,
                        "No clash groups were created (no clashes within the specified distance).",
                        "Group Clashes by Distance");
                    return 0;
                }

                int createdGroups = ApplyGroupsToTest(doc, testsData, selectedTest, groups, threshold);

                MessageBox.Show(
                    owner,
                    "Created " + createdGroups + " proximity group(s) in test:\n" +
                    "\"" + selectedTest.DisplayName + "\"\n\n" +
                    "Distance threshold: " + threshold.ToString("0.###", CultureInfo.InvariantCulture) + " m\n\n" +
                    "You can now open Clash Detective -> Results and export your report.",
                    "Group Clashes by Distance");
            }

            return 0;
        }

        // Grouping algorithm: connected components via BFS.
        // Two clashes are connected if their 3D centres are within the threshold distance.
        private static List<List<ClashResult>> GroupResultsByDistance(
            List<ClashResult> results,
            double threshold)
        {
            var groups = new List<List<ClashResult>>();
            int n = results.Count;
            if (n == 0)
                return groups;

            Point3D[] centers = new Point3D[n];
            for (int i = 0; i < n; i++)
                centers[i] = results[i].Center;

            bool[] visited = new bool[n];

            for (int i = 0; i < n; i++)
            {
                if (visited[i]) continue;

                var groupIndices = new List<int>();
                var queue = new Queue<int>();

                visited[i] = true;
                queue.Enqueue(i);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    groupIndices.Add(current);

                    for (int j = 0; j < n; j++)
                    {
                        if (visited[j]) continue;

                        double distance = Distance(centers[current], centers[j]);
                        if (distance <= threshold)
                        {
                            visited[j] = true;
                            queue.Enqueue(j);
                        }
                    }
                }

                var group = new List<ClashResult>();
                foreach (int idx in groupIndices)
                    group.Add(results[idx]);

                groups.Add(group);
            }

            return groups;
        }

        private static double Distance(Point3D a, Point3D b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            double dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static int ApplyGroupsToTest(
            Document doc,
            DocumentClashTests testsData,
            ClashTest test,
            List<List<ClashResult>> groups,
            double threshold)
        {
            int createdGroups = 0;

            using (Transaction tx = doc.BeginTransaction("Group Clashes by Distance"))
            {
                int groupNumber = 1;

                foreach (List<ClashResult> group in groups)
                {
                    if (group == null || group.Count <= 1)
                        continue;

                    ClashResultGroup tempGroup = new ClashResultGroup();
                    tempGroup.DisplayName =
                        "Proximity Clash Group " +
                        threshold.ToString("0.###", CultureInfo.InvariantCulture) +
                        "m " + groupNumber + " (" + group.Count + " clashes)";

                    int insertIndex = test.Children.Count;
                    testsData.TestsInsertCopy(test, insertIndex, tempGroup);

                    ClashResultGroup newGroup = test.Children[insertIndex] as ClashResultGroup;
                    if (newGroup == null)
                        continue;

                    foreach (ClashResult clash in group)
                    {
                        int oldIndex = FindChildIndexByGuid(test, clash.Guid);
                        if (oldIndex < 0)
                            continue;

                        int newIndex = newGroup.Children.Count;
                        testsData.TestsMove(test, oldIndex, newGroup, newIndex);
                    }

                    createdGroups++;
                    groupNumber++;
                }

                tx.Commit();
            }

            return createdGroups;
        }

        private static int FindChildIndexByGuid(GroupItem parent, Guid clashGuid)
        {
            SavedItemCollection children = parent.Children;
            for (int i = 0; i < children.Count; i++)
            {
                ClashResult cr = children[i] as ClashResult;
                if (cr != null && cr.Guid == clashGuid)
                    return i;
            }
            return -1;
        }
    }

    public class GroupClashesByDistanceForm : Form
    {
        private readonly List<ClashTest> _tests;
        private ComboBox _comboTests;
        private TextBox _txtDistance;
        private Button _btnOk;
        private Button _btnCancel;

        public ClashTest SelectedTest { get; private set; }
        public double DistanceThreshold { get; private set; }

        public GroupClashesByDistanceForm(List<ClashTest> tests)
        {
            _tests = tests != null ? tests : new List<ClashTest>();

            Text = "Group Clashes by Distance";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 420;
            Height = 200;

            Label lblTest = new Label();
            lblTest.Text = "Clash test:";
            lblTest.Left = 10;
            lblTest.Top = 20;
            lblTest.Width = 80;

            _comboTests = new ComboBox();
            _comboTests.Left = 100;
            _comboTests.Top = 16;
            _comboTests.Width = 290;
            _comboTests.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (ClashTest t in _tests)
                _comboTests.Items.Add(t.DisplayName);

            if (_comboTests.Items.Count > 0)
                _comboTests.SelectedIndex = 0;

            Label lblDistance = new Label();
            lblDistance.Text = "Distance (m):";
            lblDistance.Left = 10;
            lblDistance.Top = 60;
            lblDistance.Width = 80;

            _txtDistance = new TextBox();
            _txtDistance.Left = 100;
            _txtDistance.Top = 56;
            _txtDistance.Width = 80;
            _txtDistance.Text = "3.0";

            _btnOk = new Button();
            _btnOk.Text = "OK";
            _btnOk.Left = 220;
            _btnOk.Top = 110;
            _btnOk.Width = 80;
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Click += OnOkClicked;

            _btnCancel = new Button();
            _btnCancel.Text = "Cancel";
            _btnCancel.Left = 310;
            _btnCancel.Top = 110;
            _btnCancel.Width = 80;
            _btnCancel.DialogResult = DialogResult.Cancel;

            Controls.Add(lblTest);
            Controls.Add(_comboTests);
            Controls.Add(lblDistance);
            Controls.Add(_txtDistance);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private void OnOkClicked(object sender, EventArgs e)
        {
            if (_comboTests.SelectedIndex < 0 || _comboTests.SelectedIndex >= _tests.Count)
            {
                MessageBox.Show(this, "Please select a clash test.", "Group Clashes by Distance");
                DialogResult = DialogResult.None;
                return;
            }

            double d;
            if (!double.TryParse(_txtDistance.Text, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out d) || d <= 0.0)
            {
                MessageBox.Show(this, "Please enter a valid positive distance in metres.", "Group Clashes by Distance");
                DialogResult = DialogResult.None;
                return;
            }

            SelectedTest = _tests[_comboTests.SelectedIndex];
            DistanceThreshold = d;
        }
    }
}
