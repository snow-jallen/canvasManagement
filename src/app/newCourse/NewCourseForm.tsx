"use client";
import { DayOfWeekInput } from "@/components/form/DayOfWeekInput";
import SelectInput from "@/components/form/SelectInput";
import { Spinner } from "@/components/Spinner";
import { SuspenseAndErrorHandling } from "@/components/SuspenseAndErrorHandling";
import { useCourseListInTermQuery } from "@/hooks/canvas/canvasCourseHooks";
import { useCanvasTermsQuery } from "@/hooks/canvas/canvasHooks";
import {
  useCreateLocalCourseMutation,
  useLocalCoursesSettingsQuery,
} from "@/hooks/localCourse/localCoursesHooks";
import { useEmptyDirectoriesQuery } from "@/hooks/localCourse/storageDirectoryHooks";
import { CanvasCourseModel } from "@/models/canvas/courses/canvasCourseModel";
import { CanvasEnrollmentTermModel } from "@/models/canvas/enrollmentTerms/canvasEnrollmentTermModel";
import { AssignmentSubmissionType } from "@/models/local/assignment/assignmentSubmissionType";
import {
  DayOfWeek,
  LocalCourseSettings,
} from "@/models/local/localCourseSettings";
import { getCourseUrl } from "@/services/urlUtils";
import { useRouter } from "next/navigation";
import React, { useMemo, useState } from "react";

const sampleCompose = `services:
  canvas_manager:
    image: alexmickelson/canvas_management:2 # pull this image regularly
    user: 1000:1000 # userid:groupid that matches file ownership on host system (probably leave like this)
    ports:
      - 8080:3000 # hostPort:containerPort - you can change the host port if you like
    env_file:
      - .env # needs to have your CANVAS_TOKEN set
    environment:
      - TZ=America/Denver # prevents timezone issues for due dates
      # - FILE_POLLING=true # increases cpu usage, uncomment if source volumes are on ntfs
    volumes:
      - ~/projects/faculty/1430/2024-fall-alex/modules:/app/storage/UX
      - ~/projects/faculty/4850_AdvancedFE/2024-fall-alex/modules:/app/storage/advanced_frontend
`;

export default function NewCourseForm() {
  const router = useRouter();
  const today = useMemo(() => new Date(), []);
  const { data: canvasTerms } = useCanvasTermsQuery(today);
  const [selectedTerm, setSelectedTerm] = useState<
    CanvasEnrollmentTermModel | undefined
  >();
  const [selectedDaysOfWeek, setSelectedDaysOfWeek] = useState<DayOfWeek[]>([]);
  const [selectedCanvasCourse, setSelectedCanvasCourse] = useState<
    CanvasCourseModel | undefined
  >();
  const [selectedDirectory, setSelectedDirectory] = useState<
    string | undefined
  >();
  const [courseToImport, setCourseToImport] = useState<
    LocalCourseSettings | undefined
  >();
  const createCourse = useCreateLocalCourseMutation();

  const formIsComplete =
    selectedTerm && selectedCanvasCourse && selectedDirectory;

  return (
    <div>
      <SelectInput
        value={selectedTerm}
        setValue={setSelectedTerm}
        label={"Canvas Term"}
        options={canvasTerms}
        getOptionName={(t) => t.name}
      />
      <SuspenseAndErrorHandling>
        {selectedTerm && (
          <OtherSettings
            selectedTerm={selectedTerm}
            selectedCanvasCourse={selectedCanvasCourse}
            setSelectedCanvasCourse={setSelectedCanvasCourse}
            selectedDirectory={selectedDirectory}
            setSelectedDirectory={setSelectedDirectory}
            selectedDaysOfWeek={selectedDaysOfWeek}
            setSelectedDaysOfWeek={setSelectedDaysOfWeek}
            courseToImport={courseToImport}
            setCourseToImport={setCourseToImport}
          />
        )}
      </SuspenseAndErrorHandling>
      <div className="m-3 text-center">
        <button
          disabled={!formIsComplete || createCourse.isPending}
          onClick={async () => {
            if (formIsComplete) {
              const newSettings: LocalCourseSettings = courseToImport
                ? {
                    ...courseToImport,
                    name: selectedDirectory,
                    daysOfWeek: selectedDaysOfWeek,
                    canvasId: selectedCanvasCourse.id,
                    startDate: selectedTerm.start_at ?? "",
                    endDate: selectedTerm.end_at ?? "",
                    holidays: [],
                    assignmentGroups: courseToImport.assignmentGroups.map(
                      (assignmentGroup) => {
                        const { canvasId: _, ...groupWithoutCanvas } =
                          assignmentGroup;
                        return { ...groupWithoutCanvas, canvasId: undefined };
                      }
                    ),
                    assets: [],
                  }
                : {
                    name: selectedDirectory,
                    assignmentGroups: [],
                    daysOfWeek: selectedDaysOfWeek,
                    canvasId: selectedCanvasCourse.id,
                    startDate: selectedTerm.start_at ?? "",
                    endDate: selectedTerm.end_at ?? "",
                    defaultDueTime: { hour: 23, minute: 59 },
                    defaultAssignmentSubmissionTypes: [
                      AssignmentSubmissionType.ONLINE_TEXT_ENTRY,
                      AssignmentSubmissionType.ONLINE_UPLOAD,
                    ],
                    defaultFileUploadTypes: ["pdf", "png", "jpg", "jpeg"],
                    defaultLockHoursOffset: 0,
                    holidays: [],
                    assets: [],
                  };
              await createCourse.mutateAsync({
                settings: newSettings,
                settingsFromCourseToImport: courseToImport,
              });
              router.push(getCourseUrl(selectedDirectory));
            }
          }}
        >
          Save New Course Configuration
        </button>
      </div>
      {createCourse.isPending && <Spinner />}

      <pre>
        <div>Example docker compose</div>
        <code className="language-yml">{sampleCompose}</code>
      </pre>
    </div>
  );
}

function OtherSettings({
  selectedTerm,
  selectedCanvasCourse,
  setSelectedCanvasCourse,
  selectedDirectory,
  setSelectedDirectory,
  selectedDaysOfWeek,
  setSelectedDaysOfWeek,
  courseToImport,
  setCourseToImport,
}: {
  selectedTerm: CanvasEnrollmentTermModel;
  selectedCanvasCourse: CanvasCourseModel | undefined;
  setSelectedCanvasCourse: React.Dispatch<
    React.SetStateAction<CanvasCourseModel | undefined>
  >;
  selectedDirectory: string | undefined;
  setSelectedDirectory: React.Dispatch<
    React.SetStateAction<string | undefined>
  >;
  selectedDaysOfWeek: DayOfWeek[];
  setSelectedDaysOfWeek: React.Dispatch<React.SetStateAction<DayOfWeek[]>>;
  courseToImport: LocalCourseSettings | undefined;
  setCourseToImport: React.Dispatch<
    React.SetStateAction<LocalCourseSettings | undefined>
  >;
}) {
  const { data: canvasCourses } = useCourseListInTermQuery(selectedTerm.id);
  const { data: allSettings } = useLocalCoursesSettingsQuery();
  const { data: emptyDirectories } = useEmptyDirectoriesQuery();

  const populatedCanvasCourseIds = allSettings?.map((s) => s.canvasId) ?? [];
  const availableCourses =
    canvasCourses?.filter(
      (canvas: CanvasCourseModel) =>
        !populatedCanvasCourseIds.includes(canvas.id)
    ) ?? [];

  return (
    <>
      <SelectInput
        value={selectedCanvasCourse}
        setValue={setSelectedCanvasCourse}
        label={"Course"}
        options={availableCourses}
        getOptionName={(c) => c.name}
      />
      <SelectInput
        value={selectedDirectory}
        setValue={setSelectedDirectory}
        label={"Storage Folder"}
        options={emptyDirectories ?? []}
        getOptionName={(d) => d}
        emptyOptionText="--- add a new folder to your docker compose to add more folders ---"
      />
      <div className="px-5">
        New folders will not be created automatically, you are expected to mount
        a docker volume for each courses.
      </div>
      <br />
      <div className="flex justify-center">
        <DayOfWeekInput
          selectedDays={selectedDaysOfWeek}
          updateSettings={(day) => {
            setSelectedDaysOfWeek((oldDays) => {
              const hasDay = oldDays.includes(day);

              return hasDay
                ? oldDays.filter((d) => d !== day)
                : [day, ...oldDays];
            });
          }}
        />
      </div>
      <SelectInput
        value={courseToImport}
        setValue={setCourseToImport}
        label={"(Optional) Course Content to Import"}
        options={allSettings}
        getOptionName={(c) => c.name}
      />
      <div className="px-5">
        Assignments, Quizzes, Pages, and Lectures will have their due dates
        moved based on how far they are from the start of the semester.
        <br />
        You will still need to go through and re-order the course content, but
        things will be within a few days of where they should be.
      </div>
    </>
  );
}
