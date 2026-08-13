import type { DiscoveryAnswers } from "@/lib/dream";

export type AnswerKey = keyof DiscoveryAnswers;

export interface TextQuestion {
  type: "text";
  key: Exclude<AnswerKey, "feelings">;
  stage: 1 | 2;
  prompt: string;
  placeholder: string;
}

export interface ChipsQuestion {
  type: "chips";
  key: "feelings";
  stage: 1;
  prompt: string;
  options: string[];
}

export type OnboardingQuestion = TextQuestion | ChipsQuestion;

/** Ordered exactly as docs/02-user-journey.md Stage 1 then Stage 2. */
export const onboardingQuestions: OnboardingQuestion[] = [
  {
    type: "text",
    key: "typicalWeek",
    stage: 1,
    prompt: "What does a typical week look like for you right now?",
    placeholder: "Walk me through it: work, routines, the stuff in between.",
  },
  {
    type: "chips",
    key: "feelings",
    stage: 1,
    prompt: "Which of these feel true today?",
    options: [
      "restless",
      "curious",
      "stretched thin",
      "ready for a change",
      "comfortable but bored",
      "energized",
      "uncertain",
    ],
  },
  {
    type: "text",
    key: "doWithoutPay",
    stage: 1,
    prompt: "What do you find yourself doing even when nobody's paying you to do it?",
    placeholder: "The thing you'd do anyway.",
  },
  {
    type: "text",
    key: "problemsNoticed",
    stage: 1,
    prompt: "What kind of problems do you notice that other people seem to walk past?",
    placeholder: "Things that bug you that don't seem to bug anyone else.",
  },
  {
    type: "text",
    key: "admiredWork",
    stage: 1,
    prompt: "Whose work do you admire, and what specifically about it?",
    placeholder: "A person, a company, a project, and why it stuck with you.",
  },
  {
    type: "text",
    key: "ifMoneyWerentFactor",
    stage: 1,
    prompt: "If money weren't a factor for the next two years, how would you spend your time?",
    placeholder: "No wrong answers here.",
  },
  {
    type: "text",
    key: "driftedAwayFrom",
    stage: 1,
    prompt: "What's something you used to enjoy that you've drifted away from?",
    placeholder: "A hobby, a subject, a way of spending time.",
  },
  {
    type: "text",
    key: "drainingWork",
    stage: 1,
    prompt: "What kind of work drains you, regardless of the pay or title?",
    placeholder: "Useful to know what to avoid, too.",
  },
  {
    type: "text",
    key: "futureExperience",
    stage: 1,
    prompt: "What experience do you want to be able to talk about in five years?",
    placeholder: "Something you did, built, or became.",
  },
  {
    type: "text",
    key: "whatWouldYouChange",
    stage: 2,
    prompt: "What would you love to change, even in a small corner of the world?",
    placeholder: "Doesn't have to be big.",
  },
  {
    type: "text",
    key: "problemToSolve",
    stage: 2,
    prompt: "What problem would you love to be the person who solved it?",
    placeholder: "The one that keeps coming back to mind.",
  },
  {
    type: "text",
    key: "spendTimeDoing",
    stage: 2,
    prompt: "What would you spend your time doing if you knew you couldn't fail?",
    placeholder: "Let yourself answer this one honestly.",
  },
  {
    type: "text",
    key: "proudInFiveYears",
    stage: 2,
    prompt: "What would make you proud when you look back in five years?",
    placeholder: "Picture yourself looking back.",
  },
  {
    type: "text",
    key: "regretNeverTrying",
    stage: 2,
    prompt: "What would you regret never having tried?",
    placeholder: "The one you'd kick yourself for skipping.",
  },
  {
    type: "text",
    key: "impactOnOthers",
    stage: 2,
    prompt: "What kind of impact do you want your work to have on other people?",
    placeholder: "Who's affected, and how?",
  },
];
