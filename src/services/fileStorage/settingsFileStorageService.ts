import {
  LocalCourseSettings,
  localCourseYamlUtils,
} from "@/models/local/localCourseSettings";
import { promises as fs } from "fs";
import path from "path";
import {
  basePath,
  directoryOrFileExists,
  getCourseNames,
} from "./utils/fileSystemUtils";
import { AssignmentSubmissionType } from "@/models/local/assignment/assignmentSubmissionType";

const getCourseSettings = async (
  courseName: string
): Promise<LocalCourseSettings> => {
  const courseDirectory = path.join(basePath, courseName);
  const settingsPath = path.join(courseDirectory, "settings.yml");
  if (!(await directoryOrFileExists(settingsPath))) {
    const errorMessage = `could not find settings for ${courseName}, settings file ${settingsPath}`;
    console.log(errorMessage);
    throw new Error(errorMessage);
  }

  const settingsString = await fs.readFile(settingsPath, "utf-8");

  const settingsFromFile =
    localCourseYamlUtils.parseSettingYaml(settingsString);

  const settings: LocalCourseSettings = populateDefaultValues(settingsFromFile);

  const folderName = path.basename(courseDirectory);
  return { ...settings, name: folderName };
};

const populateDefaultValues = (settingsFromFile: LocalCourseSettings) => {
  const defaultSubmissionType = [
    AssignmentSubmissionType.ONLINE_TEXT_ENTRY,
    AssignmentSubmissionType.ONLINE_UPLOAD,
  ];
  const defaultFileUploadTypes = ["pdf", "jpg", "jpeg"];

  const settings: LocalCourseSettings = {
    ...settingsFromFile,
    defaultAssignmentSubmissionTypes:
      settingsFromFile.defaultAssignmentSubmissionTypes ||
      defaultSubmissionType,
    defaultFileUploadTypes:
      settingsFromFile.defaultFileUploadTypes || defaultFileUploadTypes,
    holidays: Array.isArray(settingsFromFile.holidays)
      ? settingsFromFile.holidays
      : [],
    assets: Array.isArray(settingsFromFile.assets)
      ? settingsFromFile.assets
      : [],
  };
  return settings;
};

export const settingsFileStorageService = {
  getCourseSettings,
  async getAllCoursesSettings() {
    const courses = await getCourseNames();

    const courseSettings = await Promise.all(
      courses.map(async (c) => await getCourseSettings(c))
    );
    return courseSettings;
  },

  async updateCourseSettings(
    courseName: string,
    settings: LocalCourseSettings
  ) {
    const courseDirectory = path.join(basePath, courseName);
    const settingsPath = path.join(courseDirectory, "settings.yml");

    const { name: _, ...settingsWithoutName } = settings;

    const settingsMarkdown =
      localCourseYamlUtils.settingsToYaml(settingsWithoutName);

    console.log(`Saving settings ${settingsPath}`);
    await fs.writeFile(settingsPath, settingsMarkdown);
  },
};
