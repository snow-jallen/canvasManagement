"use client";

import { getLecturePreviewUrl } from "@/services/urlUtils";
import Link from "next/link";
import { useCourseContext } from "../course/[courseName]/context/courseContext";
import { useLecturesSuspenseQuery as useLecturesQuery } from "@/hooks/localCourse/lectureHooks";
import { getLectureForDay } from "@/models/local/utils/lectureUtils";
import { getDateOnlyMarkdownString } from "@/models/local/utils/timeUtils";

export default function OneCourseLectures() {
  const { courseName } = useCourseContext();
  const {data: weeks} = useLecturesQuery();

  const dayAsDate = new Date();
  const dayAsString = getDateOnlyMarkdownString(dayAsDate);
  const todaysLecture = getLectureForDay(weeks, dayAsDate);

  if (!todaysLecture) return <></>;
  return (
    <Link
      href={getLecturePreviewUrl(courseName, dayAsString)}
      shallow={true}
      prefetch={false}
      className="
        border-4 rounded-lg border-slate-500 
        px-3 py-1 m-3 block text-end
        bg-slate-950
        transition-all hover:scale-110 hover:shadow-md
      "
    >
      <span className="text-end text-slate-500">lecture</span>
      <br />
      <span className="font-bold text-xl">{todaysLecture?.name}</span>
      <br />
      <span className="text-slate-500">{courseName}</span>
    </Link>
  );
}
